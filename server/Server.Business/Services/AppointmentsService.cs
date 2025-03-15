using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class AppointmentsService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;

    public AppointmentsService(UnitOfWorks unitOfWorks, IMapper mapper)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
    }

    public async Task<GetAllAppointmentResponse> GetAllAppointments(int page = 1, int pageSize = 5)
    {
        var listAppointments = await _unitOfWorks.AppointmentsRepository
            .FindByCondition(x => x.Status == "Pending" || x.Status == "Completed")
            .OrderByDescending(x => x.AppointmentId)
            .Include(x => x.Customer)
            .Include(x => x.Staff)
            .Include(x => x.Branch)
            .Include(x => x.Service)
            .ToListAsync();
        if (listAppointments.Equals(null))
        {
            return null;
        }

        var totalCount = listAppointments.Count();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pagedServices = listAppointments.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var appointmentsModels = _mapper.Map<List<AppointmentsModel>>(pagedServices);

        return new GetAllAppointmentResponse()
        {
            data = appointmentsModels,
            pagination = new Pagination
            {
                page = page,
                totalPage = totalPages,
                totalCount = totalCount
            }
        };
    }

    public async Task<AppointmentsModel> GetAppointmentsById(int id)
    {
        var appointmentsExist = await _unitOfWorks.AppointmentsRepository
            .FindByCondition(x => x.AppointmentId.Equals(id))
            .Include(x => x.Customer)
            .Include(x => x.Staff)
            .Include(x => x.Branch)
            .Include(x => x.Service)
            .FirstOrDefaultAsync();
        if (appointmentsExist == null)
        {
            return null;
        }

        return _mapper.Map<AppointmentsModel>(appointmentsExist);
    }

    public async Task<List<AppointmentsModel>> CreateAppointments(int userId, ApointmentRequest request)
    {
        await _unitOfWorks.BeginTransactionAsync();

        try
        {
            var customer = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            var branch = await _unitOfWorks.BranchRepository.FirstOrDefaultAsync(x => x.BranchId == request.BranchId);
            var voucher =
                await _unitOfWorks.VoucherRepository.FirstOrDefaultAsync(x => x.VoucherId == request.VoucherId);

            if (customer == null) throw new BadRequestException("Customer not found!");
            if (branch == null) throw new BadRequestException("Branch not found!");

            var randomOrderCode = new Random().Next(100000, 999999);

            var order = new Order
            {
                OrderCode = randomOrderCode,
                CustomerId = userId,
                TotalAmount = 0,
                OrderType = "Appointment",
                VoucherId = request.VoucherId > 0 ? request.VoucherId : null,
                DiscountAmount = voucher?.DiscountAmount,
                Status = OrderStatusEnum.Pending.ToString(),
                Note = request.Notes,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var createdOrder = await _unitOfWorks.OrderRepository.AddAsync(order);
            await _unitOfWorks.OrderRepository.Commit();

            var appointments = new List<AppointmentsModel>();

            if (request.ServiceId.Length != request.StaffId.Length)
            {
                throw new BadRequestException("The number of services and staff must match!");
            }

            var staffAppointments = new Dictionary<int, DateTime>(); // Lưu thời gian kết thúc của staff

            for (int i = 0; i < request.ServiceId.Length; i++)
            {
                var serviceId = request.ServiceId[i];
                var staffId = request.StaffId[i];

                var service = await _unitOfWorks.ServiceRepository.FirstOrDefaultAsync(x => x.ServiceId == serviceId);
                if (service == null)
                {
                    throw new BadRequestException($"Service not found!");
                }

                var serviceInBranch = await _unitOfWorks.Branch_ServiceRepository
                    .FirstOrDefaultAsync(sb => sb.ServiceId == serviceId && sb.BranchId == request.BranchId);
                if (serviceInBranch == null)
                {
                    throw new BadRequestException($"Service is not available in this branch!");
                }

                var staff = await _unitOfWorks.StaffRepository.FirstOrDefaultAsync(x => x.StaffId == staffId);
                if (staff == null)
                {
                    throw new BadRequestException($"Staff not found!");
                }

                if (staff.BranchId != request.BranchId)
                {
                    throw new BadRequestException($"Staff does not belong to branch ID {request.BranchId}!");
                }

                // Kiểm tra lịch làm việc của staff
                var totalDuration = int.Parse(service.Duration) + 5;

                if (!staffAppointments.ContainsKey(staffId))
                {
                    staffAppointments[staffId] =
                        request.AppointmentsTime; // Nếu chưa có lịch trước đó thì lấy thời gian request
                }

                var currentAppointmentTime = staffAppointments[staffId]; // Lấy thời gian bắt đầu mới nhất của staff
                var endTime = currentAppointmentTime.AddMinutes(totalDuration);

                var isStaffBusy = await _unitOfWorks.AppointmentsRepository
                    .FirstOrDefaultAsync(a => a.StaffId == staffId &&
                                              a.AppointmentsTime < endTime &&
                                              a.AppointmentEndTime > currentAppointmentTime) != null;
                if (isStaffBusy)
                {
                    throw new BadRequestException($"Staff is busy during this time!");
                }

                var newAppointment = new AppointmentsModel
                {
                    CustomerId = userId,
                    OrderId = createdOrder.OrderId,
                    StaffId = staffId,
                    ServiceId = serviceId,
                    Status = OrderStatusEnum.Pending.ToString(),
                    BranchId = request.BranchId,
                    AppointmentsTime = currentAppointmentTime,
                    AppointmentEndTime = endTime,
                    Quantity = 1,
                    UnitPrice = service.Price,
                    SubTotal = service.Price,
                    Feedback = request.Feedback ?? "",
                    Notes = request.Notes ?? ""
                };

                var appointmentEntity =
                    await _unitOfWorks.AppointmentsRepository.AddAsync(_mapper.Map<Appointments>(newAppointment));
                await _unitOfWorks.AppointmentsRepository.Commit();
                appointments.Add(_mapper.Map<AppointmentsModel>(appointmentEntity));

                // Cập nhật thời gian mới nhất của nhân viên
                staffAppointments[staffId] = endTime;
            }

            var totalAmount = appointments.Sum(a => a.SubTotal);
            createdOrder.TotalAmount = totalAmount;
            _unitOfWorks.OrderRepository.Update(createdOrder);
            var result = await _unitOfWorks.SaveChangesAsync();

            await _unitOfWorks.CommitTransactionAsync();

            return result > 0 ? appointments : null;
        }
        catch (Exception e)
        {
            await _unitOfWorks.RollbackTransactionAsync();
            throw new BadRequestException(e.Message);
        }
    }


    public async Task<AppointmentsModel> UpdateAppointments(AppointmentsModel appointmentsModel,
        AppointmentUpdateRequest request)
    {
        var customer = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.CustomerId);
        var staff = await _unitOfWorks.StaffRepository.FirstOrDefaultAsync(x => x.StaffId == request.StaffId);
        var service = await _unitOfWorks.ServiceRepository.FirstOrDefaultAsync(x => x.ServiceId == request.ServiceId);
        var branch = await _unitOfWorks.BranchRepository.FirstOrDefaultAsync(x => x.BranchId == request.BranchId);

        if (customer == null)
        {
            throw new BadRequestException("Customer not found!");
        }

        if (staff == null)
        {
            throw new BadRequestException("Staff not found!");
        }

        if (service == null)
        {
            throw new BadRequestException("Service not found!");
        }

        if (branch == null)
        {
            throw new BadRequestException("Branch not found!");
        }

        if (staff.BranchId != request.BranchId)
        {
            throw new BadRequestException("Staff does not in branch!");
        }

        if (!request.CustomerId.Equals(null))
        {
            appointmentsModel.CustomerId = request.CustomerId;
        }

        if (!request.StaffId.Equals(null))
        {
            appointmentsModel.StaffId = request.StaffId;
        }

        if (!request.ServiceId.Equals(null))
        {
            appointmentsModel.ServiceId = request.ServiceId;
        }

        if (!request.BranchId.Equals(null))
        {
            appointmentsModel.BranchId = request.BranchId;
        }

        if (!request.AppointmentsTime.Equals(null))
        {
            appointmentsModel.AppointmentsTime = request.AppointmentsTime;
        }

        if (!request.Status.Equals(null))
        {
            appointmentsModel.Status = request.Status;
        }

        if (!request.Notes.Equals(null))
        {
            appointmentsModel.Notes = request.Notes;
        }

        if (!request.Feedback.Equals(null))
        {
            appointmentsModel.Feedback = request.Feedback;
        }

        var appointmentsEntity =
            _unitOfWorks.AppointmentsRepository.Update(_mapper.Map<Appointments>(appointmentsModel));
        var result = await _unitOfWorks.AppointmentsRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<AppointmentsModel>(appointmentsEntity);
        }

        return null;
    }

    public async Task<AppointmentsModel> DeleteAppointments(AppointmentsModel appointmentsModel)
    {
        appointmentsModel.Status = "InActive";
        var appointmentsEntity =
            _unitOfWorks.AppointmentsRepository.Update(_mapper.Map<Appointments>(appointmentsModel));
        var result = await _unitOfWorks.AppointmentsRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<AppointmentsModel>(appointmentsEntity);
        }

        return null;
    }

    public async Task<HistoryBookingResponse> BookingHistory(int userId, string status, int page = 1, int pageSize = 5)
    {
        var listOrders = await _unitOfWorks.OrderRepository.FindByCondition(x => x.CustomerId == userId)
            .Where(x => x.Status == status)
            .ToListAsync();
        if (listOrders.Equals(null))
        {
            return null;
        }

        var totalCount = listOrders.Count();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pagedServices = listOrders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var orderModels = _mapper.Map<List<OrderModel>>(pagedServices);

        return new HistoryBookingResponse()
        {
            data = orderModels,
            pagination = new Pagination
            {
                page = page,
                totalPage = totalPages,
                totalCount = totalCount
            }
        };
    }

    public async Task<AppointmentsModel> CancelBookingAppointment(AppointmentsModel appointmentsModel)
    {
        appointmentsModel.Status = "Canceled";
        var appointmentsEntity =
            _unitOfWorks.AppointmentsRepository.Update(_mapper.Map<Appointments>(appointmentsModel));
        var result = await _unitOfWorks.AppointmentsRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<AppointmentsModel>(appointmentsEntity);
        }

        return null;
    }

    public async Task<List<AppointmentsModel>> GetListAppointmentsByStaffId(int staffId, int shiftId, DateTime workDate)
    {
        var shiftOfStaff = await _unitOfWorks.ShiftRepository.FirstOrDefaultAsync(x => x.ShiftId == shiftId);
        if (shiftOfStaff == null)
        {
            return new List<AppointmentsModel>();
        }

        var listAppointments = await _unitOfWorks.AppointmentsRepository.FindByCondition(x =>
                x.StaffId == staffId &&
                x.AppointmentsTime >= workDate.Date.Add(shiftOfStaff.StartTime) &&
                x.AppointmentsTime <= workDate.Date.Add(shiftOfStaff.EndTime))
            .Include(x => x.Staff)
            .ThenInclude(x => x.StaffInfo)
            .Include(x => x.Customer)
            .Include(x => x.Branch)
            .Include(x => x.Service)
            .ToListAsync();

        return _mapper.Map<List<AppointmentsModel>>(listAppointments);
    }
}