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
        var listAppointments = await _unitOfWorks.AppointmentsRepository.FindByCondition(x => x.Status == "Active").OrderByDescending(x => x.AppointmentId)
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
        var appointmentsExist = await _unitOfWorks.AppointmentsRepository.FindByCondition(x => x.AppointmentId.Equals(id))
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
        var customer = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        var branch = await _unitOfWorks.BranchRepository.FirstOrDefaultAsync(x => x.BranchId == request.BranchId);
        var result = 0;

        if (customer == null)
        {
            throw new BadRequestException("Customer not found!");
        }

        if (branch == null)
        {
            throw new BadRequestException("Branch not found!");
        }

        var randomOrderCode = new Random().Next(100000, 999999);

        var order = new Order
        {
            OrderCode = randomOrderCode,
            CustomerId = userId,
            TotalAmount = 0,
            OrderType = "Appointment",
            Status = OrderStatusEnum.Pending.ToString(),
            Note = request.Notes,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        var createdOrder = await _unitOfWorks.OrderRepository.AddAsync(order);
        await _unitOfWorks.OrderRepository.Commit();

        var appointments = new List<AppointmentsModel>();

        foreach (var serviceId in request.ServiceId)
        {
            var service = await _unitOfWorks.ServiceRepository.FirstOrDefaultAsync(x => x.ServiceId == serviceId);
            if (service == null)
            {
                throw new BadRequestException($"Service with ID {serviceId} not found!");
            }

            var totalDuration = int.Parse(service.Duration) + 5;
            var startTime = request.AppointmentsTime;
            var endTime = startTime.AddMinutes(totalDuration);

            var serviceInBranch = await _unitOfWorks.Branch_ServiceRepository
                .FirstOrDefaultAsync(sb => sb.ServiceId == serviceId && sb.BranchId == request.BranchId);

            if (serviceInBranch == null)
            {
                throw new BadRequestException($"Service with ID {serviceId} is not available in this branch!");
            }

            var serviceCategoryId = service.ServiceCategoryId;
            var listRoom = await _unitOfWorks.RoomRepository
                .FindByCondition(r => r.ServiceCategoryId == serviceCategoryId && r.BranchId == request.BranchId).ToListAsync();

            if (!listRoom.Any())
            {
                throw new BadRequestException($"No suitable room found for the service category {serviceCategoryId} in this branch!");
            }

            Room selectedRoom = null;

            foreach (var room in listRoom)
            {
                var roomOccupied = await _unitOfWorks.AppointmentsRepository
                    .FirstOrDefaultAsync(a => a.RoomId == room.RoomId &&
                                              a.AppointmentsTime <= request.AppointmentsTime &&
                                              a.AppointmentsTime.AddMinutes(int.Parse(service.Duration) + 5) > request.AppointmentsTime);
                if (roomOccupied == null)
                {
                    selectedRoom = room;
                    break; // Dừng vòng lặp khi tìm thấy phòng phù hợp
                }
            }

            if (selectedRoom == null)
            {
                throw new BadRequestException($"No available room found for the service category {serviceCategoryId} at this time!");
            }

            foreach (var staffId in request.StaffId)
            {
                var staff = await _unitOfWorks.StaffRepository.FirstOrDefaultAsync(x => x.StaffId == staffId);
                if (staff == null)
                {
                    throw new BadRequestException($"Staff with ID {staffId} not found!");
                }

                if (staff.BranchId != request.BranchId)
                {
                    throw new BadRequestException($"Staff with ID {staffId} does not belong to branch ID {request.BranchId}!");
                }

                var existingAppointment = await _unitOfWorks.AppointmentsRepository
                    .FirstOrDefaultAsync(a => a.CustomerId == userId &&
                                              a.AppointmentsTime.Date == request.AppointmentsTime.Date &&
                                              a.AppointmentsTime.Hour == request.AppointmentsTime.Hour);
                if (existingAppointment != null)
                {
                    throw new BadRequestException("Customer already has an appointment at this time!");
                }

                var staffBusy = await _unitOfWorks.AppointmentsRepository
                    .FirstOrDefaultAsync(a => a.StaffId == staffId &&
                                              a.AppointmentsTime <= request.AppointmentsTime &&
                                              a.AppointmentsTime.AddMinutes(90) > request.AppointmentsTime);
                if (staffBusy != null)
                {
                    throw new BadRequestException($"Staff with ID {staffId} is busy during this time!");
                }

                var availableBeds = await _unitOfWorks.BedRepository
                    .FindByCondition(b => b.RoomId == selectedRoom.RoomId).ToListAsync();

                var availableBed = availableBeds.FirstOrDefault(b =>
                {
                    var bedAvailability = _unitOfWorks.BedAvailabilityRepository
                        .FirstOrDefaultAsync(ba => ba.BedId == b.BedId &&
                                                   ba.StartTime <= request.AppointmentsTime &&
                                                   ba.EndTime > request.AppointmentsTime).Result;
                    return bedAvailability == null;
                });

                if (availableBed == null)
                {
                    throw new BadRequestException("No available bed found for the selected room at this time!");
                }

                var createNewAppointment = new AppointmentsModel
                {
                    CustomerId = userId,
                    OrderId = createdOrder.OrderId,
                    StaffId = staffId,
                    ServiceId = serviceId,
                    Status = "Active",
                    BranchId = request.BranchId,
                    RoomId = selectedRoom.RoomId,
                    BedId = availableBed.BedId,
                    AppointmentsTime = request.AppointmentsTime,
                    Quantity = 1,
                    UnitPrice = service.Price,
                    SubTotal = service.Price,
                    Feedback = request.Feedback ?? "",
                    Notes = request.Notes ?? ""
                };

                var bedAvailabilityEntity = new BedAvailability
                {
                    BedId = availableBed.BedId,
                    RoomId = selectedRoom.RoomId,
                    Status = ObjectStatus.InActive.ToString(),
                    StartTime = request.AppointmentsTime,
                    EndTime = endTime
                };

                await _unitOfWorks.BedAvailabilityRepository.AddAsync(bedAvailabilityEntity);

                var appointmentsEntity = await _unitOfWorks.AppointmentsRepository.AddAsync(_mapper.Map<Appointments>(createNewAppointment));
                result = await _unitOfWorks.AppointmentsRepository.Commit();
                appointments.Add(_mapper.Map<AppointmentsModel>(appointmentsEntity));
            }
        }

        createdOrder.TotalAmount = appointments.Sum(a => a.UnitPrice);
        _unitOfWorks.OrderRepository.Update(createdOrder);
        await _unitOfWorks.OrderRepository.Commit();

        return result > 0 ? appointments : null;
    }


    
    public async Task<AppointmentsModel> UpdateAppointments(AppointmentsModel appointmentsModel, AppointmentUpdateRequest request)
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

        var appointmentsEntity = _unitOfWorks.AppointmentsRepository.Update(_mapper.Map<Appointments>(appointmentsModel));
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
        var appointmentsEntity = _unitOfWorks.AppointmentsRepository.Update(_mapper.Map<Appointments>(appointmentsModel));
        var result = await _unitOfWorks.AppointmentsRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<AppointmentsModel>(appointmentsEntity);
        }

        return null;
    }

    public async Task<GetAllAppointmentResponse> BookingAppointmentHistory(int userId, int page = 1, int pageSize = 5)
    {
        var listAppointments = await _unitOfWorks.AppointmentsRepository.FindByCondition(x => x.CustomerId == userId)
            .Include(x => x.Order)
            .Include(x => x.Staff).ThenInclude(x => x.StaffInfo)
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

    public async Task<AppointmentsModel> CancelBookingAppointment(AppointmentsModel appointmentsModel)
    {
        appointmentsModel.Status = "Canceled";
        var appointmentsEntity = _unitOfWorks.AppointmentsRepository.Update(_mapper.Map<Appointments>(appointmentsModel));
        var result = await _unitOfWorks.AppointmentsRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<AppointmentsModel>(appointmentsEntity);
        }

        return null;
    }
}