using AutoMapper;
using Google.Protobuf.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

    public RoutineService(UnitOfWorks unitOfWorks, IMapper mapper, ProductService productService,
        ServiceService serviceService)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
        _productService = productService;
        _serviceService = serviceService;
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
            .GetAll()
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
                Status = OrderStatusEnum.Pending.ToString(),
                Note = request.Note ?? "",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
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
                    var endTime = appointmentTime.AddMinutes(int.Parse(service.Duration));
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
                        CreatedDate = DateTime.Now
                    };
                    listAppointment.Add(_mapper.Map<Appointments>(newAppointment));
                    appointmentTime = endTime.AddDays(step.IntervalBeforeNextStep ?? 0);
                }

                foreach (var productStep in step.ProductRoutineSteps)
                {
                    var product = productStep.Product;
                    var newOrderDetail = new OrderDetailModels()
                    {
                        OrderId = createdOrder.OrderId,
                        ProductId = product.ProductId,
                        Quantity = 1,
                        UnitPrice = product.Price,
                        SubTotal = product.Price,
                        CreatedDate = DateTime.Now
                    };
                    listOrderDetail.Add(_mapper.Map<OrderDetail>(newOrderDetail));
                }
            }

            var userRoutine = await _unitOfWorks.UserRoutineRepository
                .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.RoutineId == routine.SkincareRoutineId);
            if (userRoutine != null)
            {
                if (userRoutine.Status == ObjectStatus.Active.ToString())
                {
                    throw new BadRequestException("Bạn đã đặt liệu trình này rồi!");
                }

                userRoutine.Status = ObjectStatus.Active.ToString();
                userRoutine.UpdatedDate = DateTime.Now;
                _unitOfWorks.UserRoutineRepository.Update(userRoutine);
                await _unitOfWorks.UserRoutineRepository.Commit();

                var listUserRoutineStep = new List<UserRoutineStep>();
                foreach (var step in routine.SkinCareRoutineSteps)
                {
                    var newUserRoutineStep = new UserRoutineStep
                    {
                        UserRoutineId = userRoutine.UserRoutineId,
                        SkinCareRoutineStepId = step.SkinCareRoutineStepId,
                        StepStatus = UserRoutineStepEnum.Pending.ToString(),
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };
                    listUserRoutineStep.Add(newUserRoutineStep);
                }

                await _unitOfWorks.UserRoutineStepRepository.AddRangeAsync(listUserRoutineStep);
            }
            else
            {
                var newUserRoutine = new UserRoutine
                {
                    UserId = request.UserId,
                    RoutineId = routine.SkincareRoutineId,
                    ProgressNotes = "",
                    Status = ObjectStatus.Active.ToString(),
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };
                await _unitOfWorks.UserRoutineRepository.AddAsync(newUserRoutine);
                await _unitOfWorks.UserRoutineRepository.Commit();

                var listUserRoutineStep = new List<UserRoutineStep>();
                foreach (var step in routine.SkinCareRoutineSteps)
                {
                    var newUserRoutineStep = new UserRoutineStep
                    {
                        UserRoutineId = newUserRoutine.UserRoutineId,
                        SkinCareRoutineStepId = step.SkinCareRoutineStepId,
                        StepStatus = UserRoutineStepEnum.Pending.ToString(),
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };
                    listUserRoutineStep.Add(newUserRoutineStep);
                }

                await _unitOfWorks.UserRoutineStepRepository.AddRangeAsync(listUserRoutineStep);
            }


            await _unitOfWorks.AppointmentsRepository.AddRangeAsync(listAppointment);
            await _unitOfWorks.OrderDetailRepository.AddRangeAsync(listOrderDetail);
            await _unitOfWorks.CommitTransactionAsync();

            return order.OrderId;
        }
        catch (Exception)
        {
            await _unitOfWorks.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<UserRoutineModel> TrackingUserRoutineByRoutineId(int routineId, int userId)
    {
        var userRoutine = await _unitOfWorks.UserRoutineRepository
            .FindByCondition(x =>
                x.RoutineId == routineId && x.UserId == userId && x.Status == ObjectStatus.Active.ToString())
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
        var listProductModel = await _productService.GetListImagesOfProduct(_mapper.Map<List<Product>>(listProduct));

        // Lấy danh sách dịch vụ từ stepModels
        var serviceRoutines = routineModel.ServiceRoutines;
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
        var listProductModel = await _productService.GetListImagesOfProduct(_mapper.Map<List<Product>>(listProduct));

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
        
        var user = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x=> x.UserId == userId)
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
            await _serviceService.GetListImagesOfServices(_mapper.Map<List<Data.Entities.Service>>(distinctServices));
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
}