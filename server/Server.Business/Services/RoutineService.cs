using AutoMapper;
using Google.Protobuf.Collections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Constants;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class RoutineService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;
    private readonly ProductService _productService;
    private readonly ServiceService _serviceService;
    private readonly StaffService _staffService;
    private readonly MongoDbService _mongoDbService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<RoutineService> _logger;

    public RoutineService(UnitOfWorks unitOfWorks, IMapper mapper, ProductService productService,
        ServiceService serviceService, StaffService staffService, MongoDbService mongoDbService,
        IHubContext<NotificationHub> hubContext, ILogger<RoutineService> logger)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
        _productService = productService;
        _serviceService = serviceService;
        _staffService = staffService;
        _mongoDbService = mongoDbService;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<SkincareRoutineModel> GetSkincareRoutineDetails(int id)
    {
        var routine = await _unitOfWorks.SkincareRoutineRepository.FindByCondition(x => x.SkincareRoutineId == id)
            .Include(x => x.ProductRoutines)
            .ThenInclude(x => x.Products)
            .ThenInclude(x => x.Category)
            .Include(x => x.ServiceRoutines)
            .ThenInclude(x => x.Service)
            .ThenInclude(x => x.ServiceCategory)
            .FirstOrDefaultAsync();
        if (routine == null) return null;

        var routineModel = _mapper.Map<SkincareRoutineModel>(routine);

        // get images of product
        var productRoutines = routine.ProductRoutines;
        var listProduct = new List<Product>();
        foreach (var productRoutine in productRoutines)
        {
            listProduct.Add(productRoutine.Products);
        }

        var listProductModel = await _productService.GetListImagesOfProduct(listProduct);

        // get image of service
        var serviceRoutines = routine.ServiceRoutines;
        var listService = new List<Data.Entities.Service>();
        foreach (var serviceRoutine in serviceRoutines)
        {
            listService.Add(serviceRoutine.Service);
        }

        var listServiceModel = await _serviceService.GetListImagesOfServices(listService);


        // map images
        foreach (var serviceRoutine in routineModel.ServiceRoutines)
        {
            foreach (var service in listServiceModel)
            {
                if (serviceRoutine.Service.ServiceId == service.ServiceId)
                {
                    serviceRoutine.Service.images = service.images;
                }
            }
        }

        foreach (var productRoutine in routineModel.ProductRoutines)
        {
            foreach (var product in listProductModel)
            {
                if (productRoutine.Products.ProductId == product.ProductId)
                {
                    productRoutine.Products.images = product.images;
                }
            }
        }

        return routineModel;
    }

    public async Task<List<SkincareRoutineModel>> GetListSkincareRoutine()
    {
        var routines = await _unitOfWorks.SkincareRoutineRepository
            .FindByCondition(x => x.Status == ObjectStatus.Active.ToString())
            .OrderByDescending(x => x.SkincareRoutineId)
            .ToListAsync();
        var routineModels = _mapper.Map<List<SkincareRoutineModel>>(routines);
        return routineModels;
    }

    public async Task<List<SkinCareRoutineStepModel>> GetListSkincareRoutineStepByRoutineId(int routineId)
    {
        var steps = await _unitOfWorks.SkinCareRoutineStepRepository
            .FindByCondition(x => x.SkincareRoutineId == routineId)
            .Include(x => x.ServiceRoutineSteps)
            .ThenInclude(x => x.Service)
            .ThenInclude(x => x.ServiceCategory)
            .Include(x => x.ProductRoutineSteps)
            .ThenInclude(x => x.Product)
            .ThenInclude(x => x.Category)
            .ToListAsync();

        var stepModels = _mapper.Map<List<SkinCareRoutineStepModel>>(steps);

        // Lấy danh sách sản phẩm từ stepModels
        var productRoutineSteps = stepModels.SelectMany(x => x.ProductRoutineSteps).ToList();
        var listProduct = productRoutineSteps.Select(pr => pr.Product).ToList();
        var listProductModel = await _productService.GetListImagesOfProduct(_mapper.Map<List<Product>>(listProduct));

        // Lấy danh sách dịch vụ từ stepModels
        var serviceRoutineSteps = stepModels.SelectMany(x => x.ServiceRoutineSteps).ToList();
        var listService = serviceRoutineSteps.Select(sr => sr.Service).ToList();
        var listServiceModel =
            await _serviceService.GetListImagesOfServices(_mapper.Map<List<Data.Entities.Service>>(listService));

        // Map images của dịch vụ
        foreach (var serviceRoutine in serviceRoutineSteps)
        {
            var serviceImage = listServiceModel.FirstOrDefault(s => s.ServiceId == serviceRoutine.Service.ServiceId);
            if (serviceImage != null)
            {
                serviceRoutine.Service.images = serviceImage.images;
            }
        }

        // Map images của sản phẩm
        foreach (var productRoutineStep in productRoutineSteps)
        {
            var productImage =
                listProductModel.FirstOrDefault(p => p.ProductId == productRoutineStep.Product.ProductId);
            if (productImage != null)
            {
                productRoutineStep.Product.images = productImage.images;
            }
        }

        return stepModels;
    }

    public async Task<List<SkincareRoutineModel>> GetListSkincareRoutineByUserId(int userId, string status)
    {
        var routines = await _unitOfWorks.UserRoutineRepository
            .FindByCondition(x => x.UserId == userId && x.Status == status)
            .Include(x => x.Routine)
            .ThenInclude(x => x.ServiceRoutines)
            .ThenInclude(x => x.Service)
            .ThenInclude(x => x.ServiceCategory)
            .Include(x => x.Routine)
            .ThenInclude(x => x.ProductRoutines)
            .ThenInclude(x => x.Products)
            .ThenInclude(x => x.Category)
            .Select(x => x.Routine)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();
        var routineModels = _mapper.Map<List<SkincareRoutineModel>>(routines);

        // Lấy danh sách sản phẩm từ stepModels
        var productRoutines = routineModels.SelectMany(x => x.ProductRoutines).ToList();
        var listProduct = productRoutines.Select(pr => pr.Products).ToList();
        var listProductModel = await _productService.GetListImagesOfProduct(_mapper.Map<List<Product>>(listProduct));

        // Lấy danh sách dịch vụ từ stepModels
        var serviceRoutines = routineModels.SelectMany(x => x.ServiceRoutines).ToList();
        var listService = serviceRoutines.Select(sr => sr.Service).ToList();
        var listServiceModel =
            await _serviceService.GetListImagesOfServices(_mapper.Map<List<Data.Entities.Service>>(listService));

        // Map images của dịch vụ
        foreach (var serviceRoutine in serviceRoutines)
        {
            var serviceImage = listServiceModel.FirstOrDefault(s => s.ServiceId == serviceRoutine.Service.ServiceId);
            if (serviceImage != null)
            {
                serviceRoutine.Service.images = serviceImage.images;
            }
        }

        // Map images của sản phẩm
        foreach (var productRoutine in productRoutines)
        {
            var productImage =
                listProductModel.FirstOrDefault(p => p.ProductId == productRoutine.Products.ProductId);
            if (productImage != null)
            {
                productRoutine.Products.images = productImage.images;
            }
        }

        return routineModels;
    }

    public async Task<int> BookCompoSkinCareRoutine(BookCompoSkinCareRoutineRequest request)
    {
        await _unitOfWorks.BeginTransactionAsync();
        try
        {
            var user = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId)
                       ?? throw new BadRequestException("Không tìm thấy người dùng nào!");
            Voucher voucher = null;
            if (request.VoucherId != null && request.VoucherId != 0)
            {
                voucher =
                    await _unitOfWorks.VoucherRepository.FirstOrDefaultAsync(x => x.VoucherId == request.VoucherId)
                    ?? throw new BadRequestException("Không tìm thấy mã giảm giá nào!");
            }

            var branch = await _unitOfWorks.BranchRepository.FirstOrDefaultAsync(x => x.BranchId == request.BranchId)
                         ?? throw new BadRequestException("Không tìm thấy chi nhánh nào!");

            if (request.AppointmentTime == null)
            {
                throw new BadRequestException("Thời gian đặt lịch không hợp lệ: Thời gian không được để trống.");
            }

            if (request.AppointmentTime < DateTime.Now)
            {
                throw new BadRequestException("Thời gian đặt lịch không hợp lệ: Thời gian phải lớn hơn hiện tại.");
            }

            var routine = await _unitOfWorks.SkincareRoutineRepository
                .FindByCondition(x => x.SkincareRoutineId == request.RoutineId)
                .Include(x => x.SkinCareRoutineSteps)
                .ThenInclude(x => x.ServiceRoutineSteps)
                .ThenInclude(x => x.Service)
                .Include(x => x.SkinCareRoutineSteps)
                .ThenInclude(x => x.ProductRoutineSteps)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync();

            if (routine == null) return 0;

            var staff = await _unitOfWorks.StaffRepository
                            .FirstOrDefaultAsync(x => x.RoleId == 3 && x.BranchId == request.BranchId)
                        ?? throw new BadRequestException("Không tìm thấy nhân viên nào!");

            var randomOrderCode = new Random().Next(100000, 999999);
            var order = new Order
            {
                OrderCode = randomOrderCode,
                CustomerId = user.UserId,
                TotalAmount = routine.TotalPrice ?? 0,
                OrderType = OrderType.Routine.ToString(),
                RoutineId = routine.SkincareRoutineId,
                VoucherId = request.VoucherId > 0 ? request.VoucherId : null,
                DiscountAmount = voucher?.DiscountAmount ?? 0,
                PaymentMethod = request.PaymentMethod,
                Status = OrderStatusEnum.Pending.ToString(),
                Note = request.Note ?? "",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            var createdOrder = await _unitOfWorks.OrderRepository.AddAsync(order);
            await _unitOfWorks.OrderRepository.Commit();

            var listAppointment = new List<Appointments>();
            var listOrderDetail = new List<OrderDetail>();

            var appointmentTime = request.AppointmentTime ?? DateTime.Now;
            foreach (var step in routine.SkinCareRoutineSteps.OrderBy(x => x.Step))
            {
                foreach (var serviceStep in step.ServiceRoutineSteps)
                {
                    var service = serviceStep.Service;
                    var endTime = appointmentTime.AddMinutes(int.Parse(service.Duration) + 5);

                    // Check if customer has overlapping appointments
                    /*var isCustomerBusy = await _unitOfWorks.AppointmentsRepository
                        .FirstOrDefaultAsync(a => a.CustomerId == user.UserId &&
                                                  a.AppointmentsTime < endTime &&
                                                  a.AppointmentEndTime > appointmentTime &&
                                                  (a.Status != OrderStatusEnum.Cancelled.ToString() &&
                                                   a.Status != OrderStatusEnum.Completed.ToString())) != null;

                    if (isCustomerBusy)
                    {
                        throw new BadRequestException(
                            $"Bạn đã có một cuộc hẹn khác trùng vào khoảng thời gian: {appointmentTime:HH:mm dd/MM/yyyy}!");
                    }*/

                    var newAppointment = new AppointmentsModel
                    {
                        CustomerId = user.UserId,
                        OrderId = createdOrder.OrderId,
                        StaffId = staff.StaffId,
                        ServiceId = service.ServiceId,
                        Status = OrderStatusEnum.Pending.ToString(),
                        BranchId = request.BranchId,
                        AppointmentsTime = appointmentTime,
                        AppointmentEndTime = endTime,
                        Quantity = 1,
                        UnitPrice = service.Price,
                        SubTotal = service.Price,
                        Feedback = "",
                        Notes = "",
                        Step = step.Step,
                        CreatedDate = DateTime.Now
                    };
                    var appointmentEntity =
                        await _unitOfWorks.AppointmentsRepository.AddAsync(_mapper.Map<Appointments>(newAppointment));
                    await _unitOfWorks.AppointmentsRepository.Commit();
                    appointmentTime = endTime;

                    // get specialist MySQL
                    var specialistMySQL = await _staffService.GetStaffById(staff.StaffId);

                    // get admin, specialist, customer from MongoDB
                    var adminMongo = await _mongoDbService.GetCustomerByIdAsync(branch.ManagerId);
                    var specialistMongo = await _mongoDbService.GetCustomerByIdAsync(specialistMySQL.StaffInfo.UserId);
                    var customerMongo = await _mongoDbService.GetCustomerByIdAsync(user.UserId);

                    // create channel
                    var channel = await _mongoDbService.CreateChannelAsync(
                        $"Channel {appointmentEntity.AppointmentId} {service.Name}", adminMongo!.Id,
                        appointmentEntity.AppointmentId);

                    // add member to channel
                    await _mongoDbService.AddMemberToChannelAsync(channel.Id, specialistMongo!.Id);
                    await _mongoDbService.AddMemberToChannelAsync(channel.Id, customerMongo!.Id);
                }

                appointmentTime = appointmentTime.AddDays(step.IntervalBeforeNextStep ?? 0);

                foreach (var productStep in step.ProductRoutineSteps)
                {
                    var product = productStep.Product;
                    var newOrderDetail = new OrderDetailModels()
                    {
                        OrderId = createdOrder.OrderId,
                        ProductId = product.ProductId,
                        Quantity = 1,
                        BranchId = branch.BranchId,
                        Step = step.Step,
                        UnitPrice = product.Price,
                        SubTotal = product.Price,
                        CreatedDate = DateTime.Now
                    };
                    listOrderDetail.Add(_mapper.Map<OrderDetail>(newOrderDetail));
                }
            }

            var existingRoutines = await _unitOfWorks.UserRoutineRepository
                .FindByCondition(x => x.UserId == request.UserId && x.RoutineId == routine.SkincareRoutineId)
                .ToListAsync();

            var userRoutineActive = existingRoutines.FirstOrDefault(x => x.Status == ObjectStatus.Active.ToString());
            if (userRoutineActive != null)
            {
                throw new BadRequestException("Bạn đã đặt liệu trình này rồi!");
            }

            var listUserRoutineStep = new List<UserRoutineStep>();
            UserRoutine targetRoutine = null;

            // Nếu đã hoàn thành, tạo routine mới
            var userRoutineCompleted =
                existingRoutines.FirstOrDefault(x => x.Status == ObjectStatus.Completed.ToString());
            if (userRoutineCompleted != null)
            {
                var totalDays = routine.SkinCareRoutineSteps
                    .OrderBy(x => x.Step)
                    .Sum(x => x.IntervalBeforeNextStep ?? 0);
                var endDate = appointmentTime.AddDays(totalDays);

                var newUserRoutine = new UserRoutine
                {
                    UserId = request.UserId,
                    RoutineId = routine.SkincareRoutineId,
                    ProgressNotes = "",
                    Status = ObjectStatus.Active.ToString(),
                    StartDate = appointmentTime,
                    EndDate = endDate,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                var createdUserRoutine = await _unitOfWorks.UserRoutineRepository.AddAsync(newUserRoutine);
                await _unitOfWorks.UserRoutineRepository.Commit();

                targetRoutine = createdUserRoutine;
            }
            else
            {
                // Nếu là Suitable, cập nhật thành Active
                var userRoutineSuitable =
                    existingRoutines.FirstOrDefault(x => x.Status == ObjectStatus.Suitable.ToString());
                if (userRoutineSuitable != null)
                {
                    userRoutineSuitable.Status = ObjectStatus.Active.ToString();
                    userRoutineSuitable.UpdatedDate = DateTime.Now;
                    _unitOfWorks.UserRoutineRepository.Update(userRoutineSuitable);
                    await _unitOfWorks.UserRoutineRepository.Commit();

                    targetRoutine = userRoutineSuitable;
                }
                else
                {
                    // Chưa có thì tạo mới hoàn toàn
                    var totalDays = routine.SkinCareRoutineSteps
                        .OrderBy(x => x.Step)
                        .Sum(x => x.IntervalBeforeNextStep ?? 0);
                    var endDate = appointmentTime.AddDays(totalDays);

                    var newUserRoutine = new UserRoutine
                    {
                        UserId = request.UserId,
                        RoutineId = routine.SkincareRoutineId,
                        ProgressNotes = "",
                        Status = ObjectStatus.Active.ToString(),
                        StartDate = appointmentTime,
                        EndDate = endDate,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    var createdUserRoutine = await _unitOfWorks.UserRoutineRepository.AddAsync(newUserRoutine);
                    await _unitOfWorks.UserRoutineRepository.Commit();

                    targetRoutine = createdUserRoutine;
                }
            }

            // Cập nhật Order với Routine vừa xử lý
            order.UserRoutineId = targetRoutine.UserRoutineId;
            _unitOfWorks.OrderRepository.Update(order);
            await _unitOfWorks.OrderRepository.Commit();

            // Tạo các step tương ứng
            var stepStartDate = appointmentTime;
            foreach (var step in routine.SkinCareRoutineSteps)
            {
                var stepEndDate = stepStartDate.AddDays(step.IntervalBeforeNextStep ?? 0);
                var newUserRoutineStep = new UserRoutineStep
                {
                    UserRoutineId = targetRoutine.UserRoutineId,
                    SkinCareRoutineStepId = step.SkinCareRoutineStepId,
                    StepStatus = UserRoutineStepEnum.Pending.ToString(),
                    StartDate = stepStartDate,
                    EndDate = stepEndDate,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                listUserRoutineStep.Add(newUserRoutineStep);
            }

            await _unitOfWorks.UserRoutineStepRepository.AddRangeAsync(listUserRoutineStep);


            /*await _unitOfWorks.AppointmentsRepository.AddRangeAsync(listAppointment);*/
            await _unitOfWorks.OrderDetailRepository.AddRangeAsync(listOrderDetail);
            await _unitOfWorks.CommitTransactionAsync();

            var userMongo = await _mongoDbService.GetCustomerByIdAsync(user.UserId)
                            ?? throw new BadRequestException("Không tìm thấy thông tin khách hàng trong MongoDB!");

            var notification = new Notifications
            {
                UserId = user.UserId,
                Content = $"Đặt lịch thành công liệu trình {routine.Name}",
                Type = "Routine",
                isRead = false,
                ObjectId = order.OrderId,
                CreatedDate = DateTime.Now,
            };

            await _unitOfWorks.NotificationRepository.AddAsync(notification);
            await _unitOfWorks.NotificationRepository.Commit();

            if (NotificationHub.TryGetConnectionId(userMongo.Id, out var connectionId))
            {
                _logger.LogInformation("User connected: {userId} => {connectionId}", userMongo.Id, connectionId);
                Console.WriteLine($"User connected: {userMongo.Id} => {connectionId}");
                await _hubContext.Clients.Client(connectionId).SendAsync("receiveNotification", notification);
            }

            return order.OrderId;
        }
        catch (Exception ex)
        {
            await _unitOfWorks.RollbackTransactionAsync();
            throw new BadRequestException(ex.Message);
        }
    }

    public async Task<UserRoutineModel> TrackingUserRoutineByRoutineId(int userRoutineId)
    {
        var userRoutine = await _unitOfWorks.UserRoutineRepository
            .FindByCondition(x =>
                x.UserRoutineId == userRoutineId &&
                (x.Status == ObjectStatus.Active.ToString() || x.Status == ObjectStatus.Completed.ToString()))
            .Include(x => x.Routine)
            .Include(x => x.User)
            .Include(x => x.UserRoutineSteps)
            .ThenInclude(x => x.SkinCareRoutineStep)
            .ThenInclude(x => x.ServiceRoutineSteps)
            .ThenInclude(x => x.Service)
            .Include(x => x.UserRoutineSteps)
            .ThenInclude(x => x.SkinCareRoutineStep)
            .ThenInclude(x => x.ProductRoutineSteps)
            .ThenInclude(x => x.Product)
            .ThenInclude(x => x.Category)
            .FirstOrDefaultAsync();
        if (userRoutine == null)
        {
            throw new BadRequestException("User routine not found");
        }

        var userRoutineModel = _mapper.Map<UserRoutineModel>(userRoutine);

        // get images of product
        var productRoutines = userRoutineModel.UserRoutineSteps
            .SelectMany(x => x.SkinCareRoutineStep.ProductRoutineSteps)
            .ToList();
        var listProduct = new List<Product>();
        foreach (var productRoutine in productRoutines)
        {
            listProduct.Add(_mapper.Map<Product>(productRoutine.Product));
        }

        var listProductModel = await _productService.GetListImagesOfProduct(listProduct);

        // get image of service
        var serviceRoutines = userRoutineModel.UserRoutineSteps
            .SelectMany(x => x.SkinCareRoutineStep.ServiceRoutineSteps)
            .ToList();
        var listService = new List<Data.Entities.Service>();
        foreach (var serviceRoutine in serviceRoutines)
        {
            listService.Add(_mapper.Map<Data.Entities.Service>(serviceRoutine.Service));
        }

        var listServiceModel = await _serviceService.GetListImagesOfServices(listService);


        // map images
        foreach (var serviceRoutine in serviceRoutines)
        {
            foreach (var service in listServiceModel)
            {
                if (serviceRoutine.Service.ServiceId == service.ServiceId)
                {
                    serviceRoutine.Service.images = service.images;
                }
            }
        }

        foreach (var productRoutine in productRoutines)
        {
            foreach (var product in listProductModel)
            {
                if (productRoutine.Product.ProductId == product.ProductId)
                {
                    productRoutine.Product.images = product.images;
                }
            }
        }

        return userRoutineModel;
    }

    public async Task<SkincareRoutineModel> GetInfoRoutineOfUserNew(int userId)
    {
        var routine = await _unitOfWorks.UserRoutineRepository
            .FindByCondition(x => x.UserId == userId && x.Status == ObjectStatus.Active.ToString())
            .Include(x => x.Routine)
            .ThenInclude(x => x.ServiceRoutines)
            .ThenInclude(x => x.Service)
            .ThenInclude(x => x.ServiceCategory)
            .Include(x => x.Routine)
            .ThenInclude(x => x.ProductRoutines)
            .ThenInclude(x => x.Products)
            .ThenInclude(x => x.Category)
            .Select(x => x.Routine)
            .OrderByDescending(x => x.CreatedDate)
            .FirstOrDefaultAsync();

        if (routine == null) return null;

        var routineModel = _mapper.Map<SkincareRoutineModel>(routine);

        // Lấy danh sách sản phẩm từ stepModels
        var productRoutines = routineModel.ProductRoutines;
        var listProduct = productRoutines.Select(pr => pr.Products).ToList();
        var listProductModel =
            await _productService.GetListImagesOfProduct(_mapper.Map<List<Product>>(listProduct));

        // Lấy danh sách dịch vụ từ stepModels
        var serviceRoutines = routineModel.ServiceRoutines;
        var listService = serviceRoutines.Select(sr => sr.Service).ToList();
        var listServiceModel =
            await _serviceService.GetListImagesOfServices(_mapper.Map<List<Data.Entities.Service>>(listService));

        // Map images của dịch vụ
        foreach (var serviceRoutine in serviceRoutines)
        {
            var serviceImage =
                listServiceModel.FirstOrDefault(s => s.ServiceId == serviceRoutine.Service.ServiceId);
            if (serviceImage != null)
            {
                serviceRoutine.Service.images = serviceImage.images;
            }
        }

        // Map images của sản phẩm
        foreach (var productRoutine in productRoutines)
        {
            var productImage =
                listProductModel.FirstOrDefault(p => p.ProductId == productRoutine.Products.ProductId);
            if (productImage != null)
            {
                productRoutine.Products.images = productImage.images;
            }
        }

        return routineModel;
    }

    public async Task<OrderModel> GetDetailOrderRoutine(int userId, int orderId)
    {
        var order = await _unitOfWorks.OrderRepository
            .FindByCondition(x =>
                x.CustomerId == userId && x.OrderId == orderId && x.OrderType == OrderType.Routine.ToString())
            .Include(x => x.Customer)
            .Include(x => x.OrderDetails)
            .ThenInclude(x => x.Product)
            .ThenInclude(x => x.Category)
            .Include(x => x.Appointments)
            .ThenInclude(x => x.Service)
            .ThenInclude(x => x.ServiceCategory)
            .FirstOrDefaultAsync();
        if (order == null) return null;

        var orderModel = _mapper.Map<OrderModel>(order);

        // lấy danh sách sản phẩm từ orderDetails
        var orderDetails = orderModel.OrderDetails;
        var listProduct = orderDetails.Select(pr => pr.Product).ToList();
        var listProductModel =
            await _productService.GetListImagesOfProduct(_mapper.Map<List<Product>>(listProduct));

        // lấy danh sách dich vụ từ appointments
        var appointments = orderModel.Appointments;
        var listService = appointments.Select(sr => sr.Service).ToList();
        var listServiceModel =
            await _serviceService.GetListImagesOfServices(_mapper.Map<List<Data.Entities.Service>>(listService));

        // Map images của dịch vụ
        foreach (var appointment in appointments)
        {
            var serviceImage = listServiceModel.FirstOrDefault(s => s.ServiceId == appointment.Service.ServiceId);
            if (serviceImage != null)
            {
                appointment.Service.images = serviceImage.images;
            }
        }

        // Map images của sản phẩm
        foreach (var orderDetail in orderDetails)
        {
            var productImage =
                listProductModel.FirstOrDefault(p => p.ProductId == orderDetail.Product.ProductId);
            if (productImage != null)
            {
                orderDetail.Product.images = productImage.images;
            }
        }

        return orderModel;
    }

    public async Task<List<BranchModel>> GetListBranchByRoutineId(int routineId)
    {
        var skincareRoutine = await _unitOfWorks.SkincareRoutineRepository
                                  .FirstOrDefaultAsync(x => x.SkincareRoutineId == routineId)
                              ?? throw new BadRequestException("Không tìm thấy liệu trình nào!");

        var serviceIds = await _unitOfWorks.ServiceRoutineRepository
            .FindByCondition(x => x.RoutineId == skincareRoutine.SkincareRoutineId)
            .Select(x => x.ServiceId)
            .ToListAsync();

        var productIds = await _unitOfWorks.ProductRoutineRepository
            .FindByCondition(x => x.RoutineId == routineId)
            .Select(x => x.ProductId)
            .ToListAsync();

        var branches = await _unitOfWorks.BranchRepository
            .FindByCondition(x => x.Status == ObjectStatus.Active.ToString())
            .Include(x => x.Branch_Products)
            .ThenInclude(bp => bp.Product)
            .Include(x => x.Branch_Services)
            .ThenInclude(bs => bs.Service)
            .ToListAsync();

        var filteredBranches = branches.Where(branch =>
        {
            var branchServiceIds = branch.Branch_Services.Select(bs => bs.ServiceId).ToHashSet();
            var branchProductIds = branch.Branch_Products.Select(bp => bp.ProductId).ToHashSet();

            bool hasAllServices = serviceIds.All(id => branchServiceIds.Contains(id));
            bool hasAllProducts = productIds.All(id => branchProductIds.Contains(id));

            return hasAllServices && hasAllProducts;
        }).ToList();

        var branchModels = _mapper.Map<List<BranchModel>>(filteredBranches);
        return branchModels;
    }

    public async Task<GetListServiceAndProductRcmResponse> GetListServiceAndProductRcm(int userId)
    {
        var routines = await _unitOfWorks.UserRoutineRepository
            .FindByCondition(x => x.UserId == userId && x.Status == ObjectStatus.Suitable.ToString())
            .Include(x => x.Routine)
            .ThenInclude(x => x.ServiceRoutines)
            .ThenInclude(x => x.Service)
            .ThenInclude(x => x.ServiceCategory)
            .Include(x => x.Routine)
            .ThenInclude(x => x.ProductRoutines)
            .ThenInclude(x => x.Products)
            .ThenInclude(x => x.Category)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();

        if (routines == null || routines.Count == 0)
            return null;

        var user = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId)
                   ?? throw new BadRequestException("Không tìm thấy người dùng nào!");

        var listService = new List<Data.Entities.Service>();
        var listProduct = new List<Product>();

        foreach (var userRoutine in routines)
        {
            if (userRoutine.Routine != null)
            {
                if (userRoutine.Routine.ServiceRoutines != null)
                {
                    foreach (var serviceRoutine in userRoutine.Routine.ServiceRoutines)
                    {
                        if (serviceRoutine.Service != null)
                            listService.Add(serviceRoutine.Service);
                    }
                }

                if (userRoutine.Routine.ProductRoutines != null)
                {
                    foreach (var productRoutine in userRoutine.Routine.ProductRoutines)
                    {
                        if (productRoutine.Products != null)
                            listProduct.Add(productRoutine.Products);
                    }
                }
            }
        }

        // Distinct theo Id
        var distinctServices = listService
            .GroupBy(x => x.ServiceId)
            .Select(g => g.First())
            .ToList();

        var distinctProducts = listProduct
            .GroupBy(x => x.ProductId)
            .Select(g => g.First())
            .ToList();

        var listServiceModel =
            await _serviceService.GetListImagesOfServices(
                _mapper.Map<List<Data.Entities.Service>>(distinctServices));
        var listProductModel =
            await _productService.GetListImagesOfProduct(_mapper.Map<List<Product>>(distinctProducts));

        var response = new GetListServiceAndProductRcmResponse
        {
            UserInfo = _mapper.Map<UserInfoModel>(user),
            Services = listServiceModel,
            Products = listProductModel
        };

        return response;
    }

    public async Task<bool> UpdateStartTimeOfRoutine(int orderId, int fromStep, DateTime startTime)
    {
        var order = await _unitOfWorks.OrderRepository
            .FindByCondition(x => x.OrderId == orderId && x.OrderType == OrderType.Routine.ToString())
            .Include(x => x.Routine)
            .ThenInclude(x => x.SkinCareRoutineSteps)
            .ThenInclude(x => x.ServiceRoutineSteps)
            .ThenInclude(x => x.Service)
            .FirstOrDefaultAsync() ?? throw new BadRequestException("Không tìm thấy đơn hàng nào!");

        var stepsToUpdate = order.Routine.SkinCareRoutineSteps
            .Where(s => s.Step >= fromStep)
            .OrderBy(s => s.Step)
            .ToList();

        if (stepsToUpdate.Count == 0)
            throw new BadRequestException("Không tìm thấy bước nào để cập nhật!");

        var serviceIds = stepsToUpdate
            .SelectMany(s => s.ServiceRoutineSteps)
            .Select(s => s.ServiceId)
            .Distinct()
            .ToList();

        var appointments = await _unitOfWorks.AppointmentsRepository
            .FindByCondition(x =>
                x.OrderId == orderId && x.ServiceId != null && serviceIds.Contains(x.ServiceId) &&
                x.Step >= fromStep)
            .OrderBy(x => x.AppointmentId)
            .ToListAsync();

        if (appointments.Count == 0)
            throw new BadRequestException("Không tìm thấy lịch hẹn nào để cập nhật!");

        if (fromStep > 1)
        {
            // Lấy step ngay trước đó
            var previousStep = order.Routine.SkinCareRoutineSteps
                .FirstOrDefault(s => s.Step == fromStep - 1);

            if (previousStep != null)
            {
                var previousServiceStep = previousStep.ServiceRoutineSteps
                    .OrderBy(s => s.Step)
                    .FirstOrDefault();

                if (previousServiceStep != null)
                {
                    var previousAppointment = await _unitOfWorks.AppointmentsRepository
                        .FindByCondition(x =>
                            x.OrderId == orderId && x.ServiceId == previousServiceStep.ServiceId &&
                            x.Step == previousStep.Step)
                        .OrderBy(x => x.AppointmentId)
                        .FirstOrDefaultAsync();

                    if (previousAppointment != null)
                    {
                        // Thời gian kết thúc của step trước
                        var previousEndTime = previousAppointment.AppointmentEndTime;

                        if (previousEndTime == null)
                        {
                            throw new BadRequestException("Lịch hẹn trước chưa có thời gian kết thúc!");
                        }

                        var expectedStartDate =
                            previousEndTime.Date.AddDays(previousStep.IntervalBeforeNextStep ?? 0);

                        if (startTime <= expectedStartDate)
                        {
                            throw new BadRequestException(
                                $"Ngày bắt đầu không hợp lệ! Phải sau bước trước {previousStep.IntervalBeforeNextStep} ngày.");
                        }
                    }
                    else
                    {
                        throw new BadRequestException("Không tìm thấy lịch hẹn của bước trước!");
                    }
                }
            }
        }

        bool isFirstStep = true;
        foreach (var step in stepsToUpdate)
        {
            var serviceSteps = step.ServiceRoutineSteps.OrderBy(s => s.Step).ToList();

            foreach (var serviceStep in serviceSteps)
            {
                // Tìm appointment đúng serviceId + đúng step
                var appointment = appointments
                    .FirstOrDefault(x => x.ServiceId == serviceStep.ServiceId && x.Step == step.Step);

                if (appointment == null)
                {
                    continue;
                }

                appointments.Remove(appointment); // Remove luôn để không update trùng lần sau

                if (isFirstStep)
                {
                    // Step đầu tiên: cập nhật cả ngày giờ
                    appointment.AppointmentsTime = startTime;

                    if (int.TryParse(serviceStep.Service.Duration, out var duration))
                    {
                        appointment.AppointmentEndTime = startTime.AddMinutes(duration + 5);
                        startTime = appointment.AppointmentEndTime;
                    }
                    else
                    {
                        throw new BadRequestException("Thời lượng dịch vụ không hợp lệ!");
                    }
                }
                else
                {
                    // Các bước sau: chỉ đổi ngày, giữ giờ phút cũ
                    var oldTime = appointment.AppointmentsTime;
                    appointment.AppointmentsTime = new DateTime(
                        startTime.Year, startTime.Month, startTime.Day,
                        oldTime.Hour, oldTime.Minute, oldTime.Second);

                    var oldEndTime = appointment.AppointmentEndTime;
                    appointment.AppointmentEndTime = new DateTime(
                        startTime.Year, startTime.Month, startTime.Day,
                        oldEndTime.Hour, oldEndTime.Minute, oldEndTime.Second);
                }

                _unitOfWorks.AppointmentsRepository.Update(appointment);

                var commitResult = await _unitOfWorks.AppointmentsRepository.Commit();
                if (commitResult <= 0)
                {
                    throw new BadRequestException("Cập nhật lịch hẹn thất bại!");
                }
            }

            // Cộng thêm ngày sau mỗi bước
            var dayInterval = step.IntervalBeforeNextStep ?? 0;
            startTime = startTime.AddDays(dayInterval);

            isFirstStep = false;
        }

        return true;
    }
}