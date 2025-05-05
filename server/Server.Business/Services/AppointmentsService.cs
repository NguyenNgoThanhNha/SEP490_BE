using AutoMapper;
using Microsoft.AspNetCore.SignalR; // ASP.NET Core SignalR
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.EntityFrameworkCore.Extensions;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Constants;
using Server.Business.Dtos;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using static Server.Business.Dtos.MonthlyBookingStatsDto;

namespace Server.Business.Services;

public class AppointmentsService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;
    private readonly StaffService _staffService;
    private readonly MongoDbService _mongoDbService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<AppointmentsService> _logger;

    public AppointmentsService(UnitOfWorks unitOfWorks, IMapper mapper, StaffService staffService,
        MongoDbService mongoDbService, IHubContext<NotificationHub> hubContext, ILogger<AppointmentsService> logger)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
        _staffService = staffService;
        _mongoDbService = mongoDbService;
        _hubContext = hubContext;
        _logger = logger;
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
            .FindByCondition(x => x.AppointmentId == id)
            .Include(x => x.Customer)
            .Include(x => x.Staff)
            .ThenInclude(s => s.StaffInfo)
            .Include(x => x.Branch)
            .Include(x => x.Service)
            .ThenInclude(s => s.ServiceRoutines)
            .ThenInclude(sr => sr.Routine)
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
            Voucher voucher = null;
            if (request.VoucherId != null && request.VoucherId > 0)
            {
                voucher = await _unitOfWorks.VoucherRepository
                              .FirstOrDefaultAsync(x => x.VoucherId == request.VoucherId)
                          ?? throw new BadRequestException("Không tìm thấy voucher!");
            }

            if (customer == null) throw new BadRequestException("Không tìm thấy thông tin khách hàng!");
            if (branch == null) throw new BadRequestException("Không tìm thấy thông tin chi nhánh!");
            if (request.ServiceId.Length != request.StaffId.Length ||
                request.ServiceId.Length != request.AppointmentsTime.Length)
            {
                throw new BadRequestException("The number of services, staff, and appointment times must match!");
            }

            var randomOrderCode = new Random().Next(100000, 999999);
            var order = new Order
            {
                OrderCode = randomOrderCode,
                CustomerId = userId,
                TotalAmount = 0,
                OrderType = OrderType.Appointment.ToString(),
                VoucherId = request.VoucherId > 0 ? request.VoucherId : null,
                DiscountAmount = voucher?.DiscountAmount ?? 0,
                Status = OrderStatusEnum.Pending.ToString(),
                PaymentMethod = request.PaymentMedhod,
                Note = request.Notes,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var createdOrder = await _unitOfWorks.OrderRepository.AddAsync(order);
            await _unitOfWorks.OrderRepository.Commit();

            if (voucher != null)
            {
                voucher.RemainQuantity -= 1;
                if (voucher.RemainQuantity < 0)
                {
                    throw new BadRequestException("Voucher đã hết hạn!");
                }

                order.DiscountAmount = voucher.DiscountAmount;
                order.VoucherId = voucher.VoucherId;
                _unitOfWorks.OrderRepository.Update(order);
                await _unitOfWorks.OrderRepository.Commit();

                _unitOfWorks.VoucherRepository.Update(voucher);
                await _unitOfWorks.VoucherRepository.Commit();
            }

            var appointments = new List<AppointmentsModel>();
            var staffAppointments = new Dictionary<int, DateTime>(); // Lưu lịch làm việc của nhân viên trong request

            for (int i = 0; i < request.ServiceId.Length; i++)
            {
                var serviceId = request.ServiceId[i];
                var staffId = request.StaffId[i];
                var appointmentTime = request.AppointmentsTime[i];

                if (appointmentTime < DateTime.Now)
                {
                    throw new BadRequestException($"Thời gian đặt lịch không hợp lệ: {appointmentTime}");
                }

                var service = await _unitOfWorks.ServiceRepository.FirstOrDefaultAsync(x => x.ServiceId == serviceId);
                if (service == null)
                {
                    throw new BadRequestException($"Không tìm thấy thông tin dịch vụ!");
                }

                var serviceInBranch = await _unitOfWorks.Branch_ServiceRepository
                    .FirstOrDefaultAsync(sb => sb.ServiceId == serviceId && sb.BranchId == request.BranchId);
                if (serviceInBranch == null)
                {
                    throw new BadRequestException($"Trong chi nhánh hiện tại không tồn tại dịch vụ này!");
                }

                var staff = await _unitOfWorks.StaffRepository.FindByCondition(x => x.StaffId == staffId)
                    .Include(x => x.StaffInfo)
                    .FirstOrDefaultAsync();
                if (staff == null)
                {
                    throw new BadRequestException($"Không tìm thấy thông tin nhân viên!");
                }

                if (staff.BranchId != request.BranchId)
                {
                    throw new BadRequestException($"Staff hiện đang không trực ở chi nhánh {request.BranchId}!");
                }

                var totalDuration = int.Parse(service.Duration) + 5; // Thời gian dịch vụ + thời gian nghỉ
                var endTime = appointmentTime.AddMinutes(totalDuration);

                if (staffAppointments.ContainsKey(staffId))
                {
                    var lastAppointmentEndTime = staffAppointments[staffId];
                    if (appointmentTime < lastAppointmentEndTime)
                    {
                        throw new BadRequestException($"Staff đang bận trong khoảng thời gian: {appointmentTime}!");
                    }
                }

                staffAppointments[staffId] = endTime;

                var isStaffBusy = await _unitOfWorks.AppointmentsRepository
                    .FirstOrDefaultAsync(a => a.StaffId == staffId &&
                                              a.AppointmentsTime < endTime &&
                                              a.AppointmentEndTime > appointmentTime &&
                                              (a.Status != OrderStatusEnum.Cancelled.ToString() &&
                                               a.Status != OrderStatusEnum.Completed.ToString())) != null;
                if (isStaffBusy)
                {
                    throw new BadRequestException($"Staff đang bận trong khoảng thời gian: {appointmentTime}!");
                }

                // Check if customer has overlapping appointments
                var isCustomerBusy = await _unitOfWorks.AppointmentsRepository
                    .FirstOrDefaultAsync(a => a.CustomerId == userId &&
                                              a.AppointmentsTime < endTime &&
                                              a.AppointmentEndTime > appointmentTime &&
                                              (a.Status != OrderStatusEnum.Cancelled.ToString() &&
                                               a.Status != OrderStatusEnum.Completed.ToString())) != null;

                if (isCustomerBusy)
                {
                    throw new BadRequestException(
                        $"Bạn đã có một cuộc hẹn khác trùng vào khoảng thời gian: {appointmentTime:HH:mm dd/MM/yyyy}!");
                }


                // Lấy ngày và thứ trong tuần từ appointmentTime
                var appointmentDate = appointmentTime.Date;
                var dayOfWeek = (int)appointmentDate.DayOfWeek;

                var workSchedule = await _unitOfWorks.WorkScheduleRepository
                    .FindByCondition(ws => ws.StaffId == staffId &&
                                           ws.WorkDate.Date == appointmentDate &&
                                           ws.DayOfWeek == dayOfWeek &&
                                           ws.Status == ObjectStatus.Active.ToString())
                    .Include(ws => ws.Shift)
                    .FirstOrDefaultAsync();

                if (workSchedule == null)
                {
                    throw new BadRequestException($"Staff không có ca làm việc vào ngày {appointmentDate:dd/MM/yyyy}.");
                }
                else
                {
                    if (workSchedule != null)
                    {
                        var appointmentTimeOfDay = appointmentTime.TimeOfDay;
                        var shift = workSchedule.Shift;

                        var isWithinShiftTime = appointmentTimeOfDay >= shift.StartTime &&
                                                appointmentTimeOfDay <= shift.EndTime;

                        if (!isWithinShiftTime)
                        {
                            throw new BadRequestException(
                                "Thời gian đặt lịch không nằm trong ca làm việc của nhân viên.");
                        }
                    }
                }

                // Kiểm tra xem thời gian hẹn có nằm trong khoảng ca làm không
                var shiftStartDateTime = appointmentDate.Add(workSchedule.Shift.StartTime);
                var shiftEndDateTime = appointmentDate.Add(workSchedule.Shift.EndTime);

                if (appointmentTime < shiftStartDateTime || endTime > shiftEndDateTime)
                {
                    throw new BadRequestException(
                        $"Thời gian đặt lịch {appointmentTime:HH:mm} không nằm trong ca làm việc ({workSchedule.Shift.ShiftName}) của nhân viên.");
                }


                var newAppointment = new AppointmentsModel
                {
                    CustomerId = userId,
                    OrderId = createdOrder.OrderId,
                    StaffId = staffId,
                    ServiceId = serviceId,
                    Status = OrderStatusEnum.Pending.ToString(),
                    BranchId = request.BranchId,
                    AppointmentsTime = appointmentTime,
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

                // // get specialist MySQL
                // var specialistMySQL = await _staffService.GetStaffById(staffId);
                //
                // // get admin, specialist, customer from MongoDB
                // var adminMongo = await _mongoDbService.GetCustomerByIdAsync(branch.ManagerId);
                // var specialistMongo = await _mongoDbService.GetCustomerByIdAsync(specialistMySQL.StaffInfo.UserId);
                // var customerMongo = await _mongoDbService.GetCustomerByIdAsync(customer.UserId);
                //
                // // create channel
                // var channel = await _mongoDbService.CreateChannelAsync(
                //     $"Channel {appointmentEntity.AppointmentId} {service.Name}", adminMongo!.Id,
                //     appointmentEntity.AppointmentId);
                //
                // // add member to channel
                // await _mongoDbService.AddMemberToChannelAsync(channel.Id, specialistMongo!.Id);
                // await _mongoDbService.AddMemberToChannelAsync(channel.Id, customerMongo!.Id);
                //
                // var userMongo = await _mongoDbService.GetCustomerByIdAsync(customer.UserId)
                //                 ?? throw new BadRequestException("Không tìm thấy thông tin khách hàng trong MongoDB!");
                //
                // // create notification
                // var notification = new Notifications()
                // {
                //     UserId = customer.UserId,
                //     Content =
                //         $"Bạn có cuộc hẹn mới với {staff.StaffInfo.FullName} vào lúc {newAppointment.AppointmentsTime}",
                //     Type = "Appointment",
                //     isRead = false,
                //     ObjectId = appointmentEntity.AppointmentId,
                //     CreatedDate = DateTime.UtcNow,
                // };
                //
                // await _unitOfWorks.NotificationRepository.AddAsync(notification);
                // await _unitOfWorks.NotificationRepository.Commit();
                //
                // if (NotificationHub.TryGetConnectionId(userMongo.Id, out var connectionId))
                // {
                //     _logger.LogInformation("User connected: {userId} => {connectionId}", userMongo.Id, connectionId);
                //     Console.WriteLine($"User connected: {userMongo.Id} => {connectionId}");
                //     await _hubContext.Clients.Client(connectionId).SendAsync("receiveNotification", notification);
                // }
                //
                // if (NotificationHub.TryGetConnectionId(specialistMongo.Id, out var connectionSpecialListId))
                // {
                //     _logger.LogInformation("User connected: {userId} => {connectionId}", specialistMongo.Id,
                //         connectionId);
                //     Console.WriteLine($"User connected: {specialistMongo.Id} => {connectionSpecialListId}");
                //     await _hubContext.Clients.Client(connectionSpecialListId)
                //         .SendAsync("receiveNotification", notification);
                // }
            }


            createdOrder.TotalAmount = appointments.Sum(a => a.SubTotal);
            _unitOfWorks.OrderRepository.Update(createdOrder);
            var result = await _unitOfWorks.SaveChangesAsync();
            await _unitOfWorks.CommitTransactionAsync();

            // Tạo và gửi notification sau khi tất cả cuộc hẹn được tạo thành công
            if (result > 0)
            {
                foreach (var appointment in appointments)
                {
                    var specialistMySQL = await _staffService.GetStaffById(appointment.StaffId);
                    var specialistMongo = await _mongoDbService.GetCustomerByIdAsync(specialistMySQL.StaffInfo.UserId);
                    var customerMongo = await _mongoDbService.GetCustomerByIdAsync(customer.UserId);
                    var adminMongo = await _mongoDbService.GetCustomerByIdAsync(branch.ManagerId);

                    var service =
                        await _unitOfWorks.ServiceRepository.FirstOrDefaultAsync(x =>
                            x.ServiceId == appointment.ServiceId);

                    // Tạo channel
                    var channel = await _mongoDbService.CreateChannelAsync(
                        $"Channel {appointment.AppointmentId} {service.Name}", adminMongo!.Id,
                        appointment.AppointmentId);

                    await _mongoDbService.AddMemberToChannelAsync(channel.Id, specialistMongo!.Id);
                    await _mongoDbService.AddMemberToChannelAsync(channel.Id, customerMongo!.Id);

                    // Tạo notification
                    var notification = new Notifications
                    {
                        UserId = customer.UserId,
                        Content =
                            $"Bạn có cuộc hẹn mới với {specialistMySQL.StaffInfo.FullName} vào lúc {appointment.AppointmentsTime}",
                        Type = "Appointment",
                        isRead = false,
                        ObjectId = appointment.AppointmentId,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _unitOfWorks.NotificationRepository.AddAsync(notification);
                    await _unitOfWorks.NotificationRepository.Commit();

                    if (NotificationHub.TryGetConnectionId(customerMongo.Id, out var connectionCustomer))
                    {
                        await _hubContext.Clients.Client(connectionCustomer)
                            .SendAsync("receiveNotification", notification);
                    }

                    if (NotificationHub.TryGetConnectionId(specialistMongo.Id, out var connectionStaff))
                    {
                        await _hubContext.Clients.Client(connectionStaff)
                            .SendAsync("receiveNotification", notification);
                    }
                }

                return appointments;
            }

            return null;
        }
        catch (Exception e)
        {
            await _unitOfWorks.RollbackTransactionAsync();
            throw new BadRequestException(e.Message);
        }
    }


    public async Task<AppointmentsModel> UpdateAppointments(int appointmentId, AppointmentUpdateRequest request)
    {
        var customer = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.CustomerId && x.RoleID == 3);
        var staff = await _unitOfWorks.StaffRepository.FindByCondition(x => x.StaffId == request.StaffId)
            .Include(x => x.StaffInfo)
            .FirstOrDefaultAsync();
        var service = await _unitOfWorks.ServiceRepository.FirstOrDefaultAsync(x => x.ServiceId == request.ServiceId);
        var branch = await _unitOfWorks.BranchRepository.FirstOrDefaultAsync(x => x.BranchId == request.BranchId);

        var appointmentEntity = await _unitOfWorks.AppointmentsRepository
                                    .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId)
                                ?? throw new BadRequestException("Không tìm thấy thông tin cuộc hẹn!");

        if (customer == null) throw new BadRequestException("Không tìm thấy thông tin khách hàng!");
        if (staff == null) throw new BadRequestException("Không tìm thấy thông tin nhân viên!");
        if (service == null) throw new BadRequestException("Không tìm thấy thông tin dịch vụ!");
        if (branch == null) throw new BadRequestException("Không tìm thấy thông tin chi nhánh!");
        if (staff.BranchId != request.BranchId)
            throw new BadRequestException("Nhân viên hiện không trực trong chi nhánh!");

        // Cập nhật các thông tin
        if (request.CustomerId != null) appointmentEntity.CustomerId = request.CustomerId;
        if (request.StaffId != null)
        {
            appointmentEntity.StaffId = request.StaffId;
            var specialistMySQL = await _staffService.GetStaffById(appointmentEntity.StaffId);
            var specialistMongo = await _mongoDbService.GetCustomerByIdAsync(specialistMySQL.StaffInfo.UserId);
            
            var channel = await _mongoDbService.GetChannelByAppointmentIdAsync(appointmentEntity.AppointmentId);
            
            await _mongoDbService.AddMemberToChannelAsync(channel.Id, specialistMongo!.Id);
        }
        if (request.ServiceId != null) appointmentEntity.ServiceId = request.ServiceId;
        if (request.BranchId != null) appointmentEntity.BranchId = request.BranchId;
        if (request.AppointmentsTime != null)
        {
            appointmentEntity.AppointmentsTime = request.AppointmentsTime;
            appointmentEntity.AppointmentEndTime =
                appointmentEntity.AppointmentsTime.AddMinutes(int.Parse(service.Duration));
        }

        if (request.Status != null) appointmentEntity.Status = request.Status;
        if (request.Notes != null) appointmentEntity.Notes = request.Notes;
        if (request.Feedback != null) appointmentEntity.Feedback = request.Feedback;

        var appointmentUpdated = _unitOfWorks.AppointmentsRepository.Update(appointmentEntity);
        var result = await _unitOfWorks.AppointmentsRepository.Commit();

        if (result <= 0) return null;

        var customerMongo = await _mongoDbService.GetCustomerByIdAsync(customer.UserId);
        var staffMongo = await _mongoDbService.GetCustomerByIdAsync(staff.UserId);

        var notification = new Notifications
        {
            UserId = customer.UserId,
            Content =
                $"Cuộc hẹn với {staff.StaffInfo.FullName} vào lúc {appointmentEntity.AppointmentsTime:HH:mm dd/MM/yyyy} đã được cập nhật.",
            Type = "Appointment",
            isRead = false,
            ObjectId = appointmentEntity.AppointmentId,
            CreatedDate = DateTime.Now
        };

        await _unitOfWorks.NotificationRepository.AddAsync(notification);
        await _unitOfWorks.NotificationRepository.Commit();

        // Gửi real-time qua SignalR
        if (NotificationHub.TryGetConnectionId(customerMongo.Id, out var connectionCustomer))
        {
            await _hubContext.Clients.Client(connectionCustomer)
                .SendAsync("receiveNotification", notification);
        }

        if (NotificationHub.TryGetConnectionId(staffMongo.Id, out var connectionStaff))
        {
            await _hubContext.Clients.Client(connectionStaff)
                .SendAsync("receiveNotification", notification);
        }

        return await GetAppointmentsById(appointmentUpdated.AppointmentId);
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
        appointmentsModel.Status = OrderStatusEnum.Cancelled.ToString();
        appointmentsModel.UpdatedDate = DateTime.Now;
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


    public async Task<GetAllAppointmentPaginationResponse> GetAppointmentsByBranchAsync(
        AppointmentFilterRequest request)
    {
        if (request.BranchId <= 0)
        {
            return new GetAllAppointmentPaginationResponse
            {
                data = new List<AppointmentDtoByBrandId>(),
                pagination = new Pagination
                {
                    page = request.Page,
                    totalPage = 0,
                    totalCount = 0
                }
            };
        }

        var query = _unitOfWorks.AppointmentsRepository
            .FindByCondition(a => a.BranchId == request.BranchId)
            .Include(a => a.Staff)
            .ThenInclude(s => s.StaffInfo)
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .OrderByDescending(a => a.AppointmentsTime);

        var totalCount = await query.CountAsync();
        var totalPage = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var pagedAppointments = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var appointmentDtos = pagedAppointments.Select(a => new AppointmentDtoByBrandId
        {
            AppointmentId = a.AppointmentId,
            OrderId = a.OrderId,

            //CustomerId = a.CustomerId,
            Customer = a.Customer == null
                ? null
                : new UserDTO
                {
                    UserId = a.Customer.UserId,
                    UserName = a.Customer.UserName,
                    FullName = a.Customer.FullName,
                    Email = a.Customer.Email,
                    Gender = a.Customer.Gender,
                    City = a.Customer.City,
                    Address = a.Customer.Address,
                    BirthDate = a.Customer.BirthDate,
                    Avatar = a.Customer.Avatar,
                    PhoneNumber = a.Customer.PhoneNumber,
                    CreatedDate = a.Customer.CreatedDate,
                    CreateBy = a.Customer.CreateBy,
                    ModifyBy = a.Customer.ModifyBy,
                    ModifyDate = a.Customer.ModifyDate,
                    Status = a.Customer.Status,
                    BonusPoint = a.Customer.BonusPoint,
                    TypeLogin = a.Customer.TypeLogin,
                    RoleID = a.Customer.RoleID
                },

            //StaffId = a.StaffId,
            Staff = a.Staff?.StaffInfo == null
                ? null
                : new UserDTO
                {
                    UserId = a.Staff.StaffInfo.UserId,
                    UserName = a.Staff.StaffInfo.UserName,
                    FullName = a.Staff.StaffInfo.FullName,
                    Email = a.Staff.StaffInfo.Email,
                    Gender = a.Staff.StaffInfo.Gender,
                    City = a.Staff.StaffInfo.City,
                    Address = a.Staff.StaffInfo.Address,
                    BirthDate = a.Staff.StaffInfo.BirthDate,
                    Avatar = a.Staff.StaffInfo.Avatar,
                    PhoneNumber = a.Staff.StaffInfo.PhoneNumber,
                    CreatedDate = a.Staff.StaffInfo.CreatedDate,
                    CreateBy = a.Staff.StaffInfo.CreateBy,
                    ModifyBy = a.Staff.StaffInfo.ModifyBy,
                    ModifyDate = a.Staff.StaffInfo.ModifyDate,
                    Status = a.Staff.StaffInfo.Status,
                    BonusPoint = a.Staff.StaffInfo.BonusPoint,
                    TypeLogin = a.Staff.StaffInfo.TypeLogin,
                    RoleID = a.Staff.StaffInfo.RoleID
                },

            ServiceId = a.ServiceId,
            Service = a.Service == null
                ? null
                : new ServiceDto
                {
                    ServiceId = a.Service.ServiceId,
                    Name = a.Service.Name,
                    Price = a.Service.Price,
                    Description = a.Service.Description,
                    Duration = a.Service.Duration,
                    Status = a.Service.Status,
                    Steps = a.Service.Steps,
                    CreatedDate = a.Service.CreatedDate,
                    UpdatedDate = a.Service.UpdatedDate,
                    ServiceCategoryId = a.Service.ServiceCategoryId
                },

            BranchId = a.BranchId,
            AppointmentsTime = a.AppointmentsTime,
            AppointmentEndTime = a.AppointmentEndTime,
            Status = a.Status,
            Notes = a.Notes,
            Feedback = a.Feedback,
            Quantity = a.Quantity,
            UnitPrice = a.UnitPrice,
            SubTotal = a.SubTotal
        }).ToList();

        return new GetAllAppointmentPaginationResponse
        {
            data = appointmentDtos,
            pagination = new Pagination
            {
                page = request.Page,
                totalPage = totalPage,
                totalCount = totalCount
            }
        };
    }

    public async Task<int> UpdateStatusAppointment(int appointmentId, string status)
    {
        var appointment = await _unitOfWorks.AppointmentsRepository
                              .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId)
                          ?? throw new BadRequestException("Không tìm thấy thông tin lịch hẹn");

        appointment.Status = status;
        appointment.UpdatedDate = DateTime.Now;

        appointment = _unitOfWorks.AppointmentsRepository.Update(appointment);
        var result = await _unitOfWorks.AppointmentsRepository.Commit();

        if (result <= 0)
            throw new BadRequestException("Cập nhật trạng thái lịch hẹn thất bại");

        // BẮN NOTIFICATION
        var customer = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x => x.UserId == appointment.CustomerId);
        var staff = await _unitOfWorks.StaffRepository
            .FindByCondition(x => x.StaffId == appointment.StaffId)
            .Include(x => x.StaffInfo)
            .FirstOrDefaultAsync();
        var service =
            await _unitOfWorks.ServiceRepository.FirstOrDefaultAsync(x => x.ServiceId == appointment.ServiceId);

        var customerMongo = await _mongoDbService.GetCustomerByIdAsync(customer!.UserId);
        var specialistMongo = await _mongoDbService.GetCustomerByIdAsync(staff!.UserId);

        var notification = new Notifications
        {
            UserId = customer.UserId,
            Content =
                $"Lịch hẹn #{appointment.AppointmentId} với dịch vụ {service!.Name} đã được cập nhật trạng thái: {status}",
            Type = "Appointment",
            isRead = false,
            ObjectId = appointment.AppointmentId,
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWorks.NotificationRepository.AddAsync(notification);
        await _unitOfWorks.NotificationRepository.Commit();

        if (NotificationHub.TryGetConnectionId(customerMongo.Id, out var connectionCustomer))
        {
            await _hubContext.Clients.Client(connectionCustomer)
                .SendAsync("receiveNotification", notification);
        }

        if (NotificationHub.TryGetConnectionId(specialistMongo.Id, out var connectionStaff))
        {
            await _hubContext.Clients.Client(connectionStaff)
                .SendAsync("receiveNotification", notification);
        }

        return appointment.AppointmentId;
    }


    //public async Task<GetAllAppointmentResponseCustomer> GetAppointmentsByCustomer(int customerId, int page = 1, int pageSize = 5)
    //{
    //    var query = _unitOfWorks.AppointmentsRepository
    //        .FindByCondition(x => x.CustomerId == customerId)
    //        .Include(x => x.Order)
    //        .Include(x => x.Service)           
    //            .ThenInclude(s => s.ServiceRoutines)
    //                .ThenInclude(sr => sr.Routine)
    //        .Include(x => x.Staff) .ThenInclude(s => s.StaffInfo)
    //        .Include(x => x.Branch)
    //        .Include(x => x.Customer)
    //        .OrderByDescending(x => x.AppointmentsTime);

    //    var totalCount = await query.CountAsync();
    //    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    //    var appointments = await query
    //        .Skip((page - 1) * pageSize)
    //        .Take(pageSize)
    //        .ToListAsync();

    //    var mappedAppointments = _mapper.Map<List<CustomerAppointmentModel>>(appointments);

    //    return new GetAllAppointmentResponseCustomer
    //    {
    //        message = "Lấy danh sách lịch hẹn của khách hàng thành công!",
    //        data = mappedAppointments,
    //        pagination = new Pagination
    //        {
    //            page = page,
    //            totalPage = totalPages,
    //            totalCount = totalCount
    //        }
    //    };
    //}

    public async Task<List<CustomerAppointmentModel>> GetAppointmentsByCustomer(int customerId, DateTime startDate,
        DateTime? endDate)
    {
        // Nếu không truyền hoặc truyền endDate là MinValue → mặc định cộng 7 ngày
        var effectiveEndDate = (!endDate.HasValue || endDate == DateTime.MinValue)
            ? startDate.AddDays(7)
            : endDate.Value;

        // Truy vấn danh sách lịch hẹn trong khoảng thời gian
        var appointments = await _unitOfWorks.AppointmentsRepository
            .FindByCondition(x => x.CustomerId == customerId
                                  && x.AppointmentsTime >= startDate
                                  && x.AppointmentsTime <= effectiveEndDate)
            .Include(x => x.Order)
            .Include(x => x.Service)
            .ThenInclude(s => s.ServiceRoutines)
            .ThenInclude(sr => sr.Routine)
            .Include(x => x.Staff)
            .ThenInclude(s => s.StaffInfo)
            .Include(x => x.Branch)
            .Include(x => x.Customer)
            .OrderByDescending(x => x.AppointmentsTime)
            .ToListAsync();

        // Mapping kết quả sang DTO
        var mappedAppointments = _mapper.Map<List<CustomerAppointmentModel>>(appointments);

        return mappedAppointments;
    }


    public async Task<YearlyBookingStatsDto> GetYearlyBookingStatsAsync(int branchId, int year)
    {
        var appointments = await _unitOfWorks.AppointmentsRepository
            .FindByCondition(a => a.BranchId == branchId && a.AppointmentsTime.Year == year)
            .ToListAsync();

        var monthlyStats = Enumerable.Range(1, 12)
            .Select(month =>
            {
                var monthlyAppointments = appointments
                    .Where(a => a.AppointmentsTime.Month == month)
                    .ToList();

                return new MonthlyStatDto
                {
                    Month = month,
                    TotalBookings = monthlyAppointments.Count,
                    TotalServicesBooked = monthlyAppointments.Sum(a => a.Quantity)
                };
            }).ToList();

        return new YearlyBookingStatsDto
        {
            BranchId = branchId,
            Year = year,
            MonthlyStats = monthlyStats
        };
    }


    public async Task<List<CustomerAppointmentModel>> GetAppointmentsByRoutine(int customerId, int routineId)
    {
        // Bước 1: Lấy order mới nhất theo routine + customer
        var latestOrder = await _unitOfWorks.OrderRepository
            .FindByCondition(x => x.CustomerId == customerId && x.RoutineId == routineId)
            .OrderByDescending(x => x.CreatedDate)
            .FirstOrDefaultAsync();

        if (latestOrder == null)
            return new List<CustomerAppointmentModel>();

        // Bước 2: Lấy danh sách lịch hẹn theo OrderId
        var appointments = await _unitOfWorks.AppointmentsRepository
            .FindByCondition(x => x.OrderId == latestOrder.OrderId)
            .Include(x => x.Service)
            .Include(x => x.Staff).ThenInclude(s => s.StaffInfo)
            .Include(x => x.Branch)
            .Include(x => x.Customer)
            .ToListAsync();

        return _mapper.Map<List<CustomerAppointmentModel>>(appointments);
    }

    public async Task<List<AppointmentsModel>> GetAppointmentsWithinNext24HoursAsync(DateTime time, int? branchId)
    {
        var staffUserIds = await _unitOfWorks.StaffRepository
            .FindByCondition(s => s.StaffInfo.RoleID == 3 && s.RoleId == 3)
            .Select(s => s.StaffId)
            .ToListAsync();

        if (!staffUserIds.Any())
            return new List<AppointmentsModel>();

        var timeEnd = time.AddDays(1);
        var query = _unitOfWorks.AppointmentsRepository
            .FindByCondition(a =>
                staffUserIds.Contains(a.StaffId) &&
                a.AppointmentsTime >= time &&
                a.AppointmentsTime <= timeEnd);
        if (branchId.HasValue)
        {
            query = query.Where(a => a.BranchId == branchId.Value);
        }

        var appointments = await query
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .Include(a => a.Branch)
            .Include(a => a.Staff)
            .ThenInclude(s => s.StaffInfo)
            .OrderBy(a => a.AppointmentsTime)
            .ToListAsync();

        return _mapper.Map<List<AppointmentsModel>>(appointments);
    }
}