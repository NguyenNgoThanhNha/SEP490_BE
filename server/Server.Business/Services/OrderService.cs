using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Constants;
using Server.Business.Dtos;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using Net.payOS;
using Net.payOS.Types;
using Server.Business.Commons.Request;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Business.Ultils;
using Server.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging; // ASP.NET Core SignalR

namespace Server.Business.Services
{
    public class OrderService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly ServiceService _serviceService;
        private readonly PayOSSetting _payOsSetting;
        private readonly AuthService _authService;
        private readonly ProductService _productService;
        private readonly StaffService _staffService;
        private readonly MongoDbService _mongoDbService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<OrderService> _logger;
        private readonly MailService _mailService;

        public OrderService(UnitOfWorks unitOfWorks, IMapper mapper, IOptions<PayOSSetting> payOsSetting,
            ServiceService serviceService, AuthService authService, ProductService productService,
            StaffService staffService, MongoDbService mongoDbService, IHubContext<NotificationHub> hubContext,
            ILogger<OrderService> logger, MailService mailService)
        {
            this._unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _serviceService = serviceService;
            _payOsSetting = payOsSetting.Value;
            _authService = authService;
            _productService = productService;
            _staffService = staffService;
            _mongoDbService = mongoDbService;
            _hubContext = hubContext;
            _logger = logger;
            _mailService = mailService;
        }

        public async Task<Pagination<Order>> GetListAsync(
            Expression<Func<Order, bool>> filter = null,
            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = null,
            string includeProperties = "",
            int? pageIndex = null,
            int? pageSize = null)
        {
            IQueryable<Order> query = _unitOfWorks.OrderRepository.FindByCondition(o => true);


            // Áp dụng bộ lọc nếu có
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Include các bảng liên quan
            foreach (var includeProperty in includeProperties.Split(
                         new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            // Sắp xếp nếu có
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Tổng số bản ghi
            var totalItemsCount = await query.CountAsync();

            // Phân trang
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                int validPageIndex = Math.Max(pageIndex.Value, 0);
                int validPageSize = Math.Max(pageSize.Value, 1);

                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }

            var items = await query.ToListAsync();

            return new Pagination<Order>
            {
                TotalItemsCount = totalItemsCount,
                PageSize = pageSize ?? totalItemsCount,
                PageIndex = pageIndex ?? 0,
                Data = items
            };
        }


        public async Task<ApiResult<object>> CreateOrderAsync(CUOrderDto model)
        {
            try
            {
                // Kiểm tra tồn tại mã đơn hàng
                var isOrderCodeExists = await _unitOfWorks.OrderRepository
                    .FindByCondition(x =>
                        x.OrderCode == model.OrderCode && x.Status == OrderStatusEnum.Pending.ToString())
                    .AnyAsync();

                if (isOrderCodeExists)
                {
                    throw new BadRequestException("Order code already exists.");
                }

                // Kiểm tra tồn tại khách hàng
                var isCustomerExists = await _unitOfWorks.UserRepository
                    .FindByCondition(x => x.UserId == model.CustomerId &&
                                          x.RoleID == (int)RoleConstant.RoleType.Customer &&
                                          x.Status == "Active")
                    .AnyAsync();

                if (!isCustomerExists)
                {
                    throw new BadRequestException("Customer not found.");
                }

                // Kiểm tra tồn tại voucher (nếu có)
                Voucher voucher = null;
                if (model.VoucherId.HasValue && model.VoucherId.Value > 0)
                {
                    var isVoucherExists = await _unitOfWorks.VoucherRepository
                        .FindByCondition(x => x.VoucherId == model.VoucherId && x.Status == "Active")
                        .FirstOrDefaultAsync();

                    if (isVoucherExists == null)
                    {
                        throw new BadRequestException("Voucher not found.");
                    }

                    voucher = isVoucherExists; // Chỉ gán nếu voucher hợp lệ
                }

                // Tạo đơn hàng mới
                var order = new Order
                {
                    OrderCode = model.OrderCode,
                    CustomerId = model.CustomerId,
                    OrderType = OrderType.Product.ToString(),
                    VoucherId = voucher?.VoucherId > 0 ? voucher.VoucherId : null,
                    DiscountAmount = voucher?.VoucherId > 0 ? voucher.DiscountAmount : 0,
                    TotalAmount = model.TotalAmount,
                    Status = OrderStatusEnum.Pending.ToString(),
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                await _unitOfWorks.OrderRepository.AddAsync(order);
                await _unitOfWorks.OrderRepository.Commit();


                // Lấy lại dữ liệu sau khi tạo
                var createdOrder = await _unitOfWorks.OrderRepository
                    .FindByCondition(o => o.OrderId == order.OrderId)
                    .Include(o => o.Customer)
                    .Include(o => o.Voucher)
                    .FirstOrDefaultAsync();


                if (createdOrder == null)
                {
                    throw new BadRequestException("Failed to retrieve created order.");
                }

                // Trả về dữ liệu
                return ApiResult<object>.Succeed(new
                {
                    OrderId = createdOrder.OrderId,
                    OrderCode = createdOrder.OrderCode,
                    CustomerName = createdOrder.Customer?.FullName ?? "N/A",
                    VoucherCode = createdOrder.Voucher?.Code,
                    TotalAmount = createdOrder.TotalAmount,
                    Status = createdOrder.Status,
                    CreatedDate = createdOrder.CreatedDate,
                    UpdatedDate = createdOrder.UpdatedDate
                });
            }
            catch (Exception ex)
            {
                throw new BadRequestException("Error creating order: " + ex.Message);
            }
        }

        public async Task<string> ConfirmOrderDetailAsync(ConfirmOrderRequest req)
        {
            var order = await _unitOfWorks.OrderRepository.GetByIdAsync(req.orderId);
            if (order == null)
            {
                throw new BadRequestException("Order not found!");
            }

            // Lấy danh sách tất cả OrderDetail theo OrderId
            var orderDetails = await _unitOfWorks.OrderDetailRepository
                .FindByCondition(x => x.OrderId == req.orderId)
                .Include(x => x.Product) // Include Product để lấy thông tin sản phẩm
                .ToListAsync();

            if (orderDetails == null || !orderDetails.Any())
            {
                throw new BadRequestException("No order details found for the given Order ID!");
            }

            // Tính tổng tiền của tất cả các OrderDetail
            var totalAmount = Convert.ToDecimal(req.totalAmount);

            //// Xác minh tổng tiền với giá trị từ request (nếu cần)
            //var requestedAmount = Convert.ToDecimal(req.totalAmount);
            //var epsilon = 0.01m;

            //if (Math.Abs(requestedAmount - totalAmount) > epsilon)
            //{
            //    throw new BadRequestException("Total amount mismatch!");
            //}


            // Khởi tạo PayOS
            var payOS = new PayOS(_payOsSetting.ClientId, _payOsSetting.ApiKey, _payOsSetting.ChecksumKey);
            var domain = _payOsSetting.Domain;

            // Tạo danh sách các item từ các OrderDetail
            var itemsList = orderDetails.Select(od => new ItemData(
                name: od.Product.ProductName,
                quantity: od.Quantity,
                price: Convert.ToInt32(od.UnitPrice) // Làm tròn giá thành số nguyên
            )).ToList();

            // Tạo OrderCode duy nhất
            var orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            order.OrderCode = orderCode;
            _unitOfWorks.OrderRepository.Update(order);
            await _unitOfWorks.OrderRepository.Commit();

            // Chuẩn bị PaymentData
            var paymentLinkRequest = new PaymentData(
                orderCode: orderCode,
                amount: Convert.ToInt32(totalAmount),
                description: $"Order {order.OrderCode}",
                items: itemsList,
                returnUrl: $"{domain}/{req.Request.returnUrl}",
                cancelUrl: $"{domain}/{req.Request.cancelUrl}"
            );

            // Thực thi PayOS và trả về link thanh toán
            try
            {
                var response = await payOS.createPaymentLink(paymentLinkRequest);
                return response.checkoutUrl;
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Failed to create payment link: {ex.Message}");
            }
        }

        public async Task<string> ConfirmOrderAppointmentAsync(ConfirmOrderRequest req)
        {
            var order = await _unitOfWorks.OrderRepository.GetByIdAsync(req.orderId);
            if (order == null)
            {
                throw new BadRequestException("Order not found!");
            }

            // Lấy danh sách tất cả Appointments theo OrderId
            var appointments = await _unitOfWorks.AppointmentsRepository
                .FindByCondition(x => x.OrderId == req.orderId)
                .Include(x => x.Service) // Include Service để lấy thông tin dịch vụ
                .Include(x => x.Branch) // Include Branch để lấy thông tin chi nhánh (nếu cần)
                .ToListAsync();

            if (appointments == null || !appointments.Any())
            {
                throw new BadRequestException("No appointments found for the given Order ID!");
            }


            // Tính tổng tiền của tất cả các Appointments
            //var totalAmount = appointments.Sum(ap => ap.Quantity * ap.UnitPrice);

            var totalAmount = Convert.ToDecimal(req.totalAmount);

            //// Xác minh tổng tiền với giá trị từ request (nếu cần)
            //if (Convert.ToDecimal(req.totalAmount) != totalAmount)
            //{
            //    throw new BadRequestException("Total amount mismatch!");
            //}

            // Khởi tạo PayOS
            var payOS = new PayOS(_payOsSetting.ClientId, _payOsSetting.ApiKey, _payOsSetting.ChecksumKey);
            var domain = _payOsSetting.Domain;

            // Tạo danh sách các item từ các Appointments
            var itemsList = appointments.Select(ap => new ItemData(
                name: ap.Service.Name, // Lấy tên dịch vụ từ Service
                quantity: ap.Quantity,
                price: Convert.ToInt32(ap.UnitPrice) // Làm tròn giá thành số nguyên
            )).ToList();

            // Tạo OrderCode duy nhất
            var orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            order.OrderCode = orderCode;
            _unitOfWorks.OrderRepository.Update(order);
            await _unitOfWorks.OrderRepository.Commit();

            // Chuẩn bị PaymentData
            var paymentLinkRequest = new PaymentData(
                orderCode: orderCode,
                amount: Convert.ToInt32(totalAmount),
                description: $"Order {order.OrderCode} - Appointments",
                items: itemsList,
                returnUrl: $"{domain}/{req.Request.returnUrl}",
                cancelUrl: $"{domain}/{req.Request.cancelUrl}"
            );

            // Thực thi PayOS và trả về link thanh toán
            try
            {
                var response = await payOS.createPaymentLink(paymentLinkRequest);
                return response.checkoutUrl;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create payment link.", ex);
            }
        }

        public async Task<string> ConfirmOrderAsync(ConfirmOrderRequest req)
        {
            var order = await _unitOfWorks.OrderRepository.GetByIdAsync(req.orderId);
            if (order == null)
                throw new BadRequestException("Order not found!");

            var totalAmount = Convert.ToDecimal(req.totalAmount);
            var itemsList = new List<ItemData>();

            /*if (order.OrderType == OrderType.Product.ToString() || order.OrderType == OrderType.Routine.ToString())
            {
                var orderDetails = await _unitOfWorks.OrderDetailRepository
                    .FindByCondition(x => x.OrderId == req.orderId)
                    .Include(x => x.Product)
                    .ToListAsync();

                if (order.OrderType == OrderType.Product.ToString() && !orderDetails.Any())
                    throw new BadRequestException("No order details found for the given Order ID!");

                itemsList.AddRange(orderDetails.Select(od => new ItemData(
                    name: od.Product.ProductName,
                    quantity: od.Quantity,
                    price: Convert.ToInt32(od.UnitPrice)
                )));
            }

            if (order.OrderType == OrderType.Appointment.ToString() || order.OrderType == OrderType.Routine.ToString())
            {
                var appointments = await _unitOfWorks.AppointmentsRepository
                    .FindByCondition(x => x.OrderId == req.orderId)
                    .Include(x => x.Service)
                    .Include(x => x.Branch)
                    .ToListAsync();

                if (order.OrderType == OrderType.Appointment.ToString() && !appointments.Any())
                    throw new BadRequestException("No appointments found for the given Order ID!");

                itemsList.AddRange(appointments.Select(ap => new ItemData(
                    name: ap.Service.Name,
                    quantity: ap.Quantity,
                    price: Convert.ToInt32(ap.UnitPrice)
                )));
            }*/

            if (order.OrderType == OrderType.Product.ToString()
                || order.OrderType == OrderType.Routine.ToString()
                || order.OrderType == OrderType.ProductAndService.ToString())
            {
                // Lấy danh sách order details (Product)
                var orderDetails = await _unitOfWorks.OrderDetailRepository
                    .FindByCondition(x => x.OrderId == req.orderId)
                    .Include(x => x.Product)
                    .ToListAsync();

                if (order.OrderType == OrderType.Product.ToString() && !orderDetails.Any())
                    throw new BadRequestException("No order details found for the given Order ID!");

                itemsList.AddRange(orderDetails.Select(od => new ItemData(
                    name: od.Product.ProductName,
                    quantity: od.Quantity,
                    price: Convert.ToInt32(od.UnitPrice)
                )));
            }

            if (order.OrderType == OrderType.Appointment.ToString()
                || order.OrderType == OrderType.Routine.ToString()
                || order.OrderType == OrderType.ProductAndService.ToString())
            {
                // Lấy danh sách appointments (Service)
                var appointments = await _unitOfWorks.AppointmentsRepository
                    .FindByCondition(x => x.OrderId == req.orderId)
                    .Include(x => x.Service)
                    .Include(x => x.Branch)
                    .ToListAsync();

                if (order.OrderType == OrderType.Appointment.ToString() && !appointments.Any())
                    throw new BadRequestException("No appointments found for the given Order ID!");

                itemsList.AddRange(appointments.Select(ap => new ItemData(
                    name: ap.Service.Name,
                    quantity: ap.Quantity,
                    price: Convert.ToInt32(ap.UnitPrice)
                )));
            }


            if (!itemsList.Any())
                throw new BadRequestException("No items found to process payment!");

            // Tạo OrderCode duy nhất
            var orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            order.OrderCode = orderCode;
            _unitOfWorks.OrderRepository.Update(order);
            await _unitOfWorks.OrderRepository.Commit();

            // Khởi tạo PayOS
            var payOS = new PayOS(_payOsSetting.ClientId, _payOsSetting.ApiKey, _payOsSetting.ChecksumKey);
            var domain = _payOsSetting.Domain;

            var paymentLinkRequest = new PaymentData(
                orderCode: orderCode,
                amount: Convert.ToInt32(totalAmount),
                description: $"Order {order.OrderCode} - {order.OrderType}",
                items: itemsList,
                returnUrl: $"{domain}/{req.Request.returnUrl}",
                cancelUrl: $"{domain}/{req.Request.cancelUrl}"
            );

            try
            {
                var response = await payOS.createPaymentLink(paymentLinkRequest);
                return response.checkoutUrl;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create payment link.", ex);
            }
        }


        public async Task<string> DepositAppointmentAsync(DepositRequest req)
        {
            // Tìm Order theo OrderId từ yêu cầu
            var order = await _unitOfWorks.OrderRepository
                .FindByCondition(o => o.OrderId == req.orderId)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new BadRequestException("Order not found!");
            }

            // Xác định phần trăm đặt cọc
            if (!decimal.TryParse(req.percent, out decimal depositPercent) || depositPercent <= 0 ||
                depositPercent > 100)
            {
                throw new BadRequestException("Invalid deposit percentage!");
            }

            // Chuyển đổi TotalAmount sang decimal
            if (!decimal.TryParse(req.totalAmount, out decimal totalAmount) || totalAmount <= 0)
            {
                throw new BadRequestException("Invalid total amount!");
            }

            // Tính số tiền đặt cọc
            decimal depositAmount = totalAmount * (depositPercent / 100);

            // Tạo OrderCode duy nhất
            var orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));

            // Cập nhật thông tin đặt cọc vào Order
            order.OrderCode = orderCode;
            order.StatusPayment = OrderStatusPaymentEnum.PendingDeposit.ToString();
            order.Note = $"Đặt cọc {depositPercent}% với số tiền: {depositAmount:C}";
            order.UpdatedDate = DateTime.Now;

            // Lưu thay đổi
            _unitOfWorks.OrderRepository.Update(order);
            await _unitOfWorks.OrderRepository.Commit();

            // Khởi tạo PayOS
            var payOS = new PayOS(_payOsSetting.ClientId, _payOsSetting.ApiKey, _payOsSetting.ChecksumKey);
            var domain = _payOsSetting.Domain;

            // Tạo PaymentData cho PayOS
            var paymentLinkRequest = new PaymentData(
                orderCode: orderCode,
                amount: Convert.ToInt32(depositAmount),
                description: $"Order {order.OrderCode} Deposit",
                items: new List<ItemData>
                {
                    new ItemData(
                        name: $"Thanh toán cọc {depositPercent}%",
                        quantity: 1,
                        price: Convert.ToInt32(depositAmount)
                    )
                },
                returnUrl: $"{domain}/{req.Request.returnUrl}",
                cancelUrl: $"{domain}/{req.Request.cancelUrl}"
            );

            // Tạo liên kết thanh toán
            try
            {
                var response = await payOS.createPaymentLink(paymentLinkRequest);
                return response.checkoutUrl;
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Failed to create payment link: {ex.Message}");
            }
        }


        //public async Task<HistoryBookingResponse> BookingHistory(int userId, string status, string orderType,
        //    int page = 1,
        //    int pageSize = 5)
        //{
        //    var listOrders = await _unitOfWorks.OrderRepository
        //        .FindByCondition(x => x.CustomerId == userId)
        //        .Include(x => x.Customer)
        //        .Include(x => x.Routine)
        //        .Include(x => x.Voucher)
        //        .Include(x => x.Shipment) // ✅ Thêm Include Shipment
        //        .Include(x => x.Appointments)
        //        .ThenInclude(x => x.Service)
        //        .Include(x => x.OrderDetails)
        //        .ThenInclude(od => od.Product)
        //        .ThenInclude(p => p.ProductImages) // ✅ Hình ảnh sản phẩm
        //        .Include(x => x.OrderDetails)
        //        .ThenInclude(od => od.Product)
        //        .ThenInclude(p => p.Branch_Products)
        //        .ThenInclude(bp => bp.Branch) // ✅ Chi nhánh của sản phẩm
        //        .Where(x => x.Status == status && x.OrderType == orderType)
        //        .OrderByDescending(x => x.CreatedDate)
        //        .ToListAsync();


        //    if (listOrders.Equals(null))
        //    {
        //        return null;
        //    }

        //    var totalCount = listOrders.Count();

        //    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        //    var pagedServices = listOrders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        //    var orderModels = _mapper.Map<List<OrderModel>>(pagedServices);

        //    return new HistoryBookingResponse()
        //    {
        //        data = orderModels,
        //        pagination = new Pagination
        //        {
        //            page = page,
        //            totalPage = totalPages,
        //            totalCount = totalCount
        //        }
        //    };
        //}

        public async Task<HistoryBookingResponse> BookingHistory(int userId, string status, string orderType,
            int page = 1, int pageSize = 5)
        {
            var listOrders = await _unitOfWorks.OrderRepository
                .FindByCondition(x => x.CustomerId == userId)
                .Include(x => x.Customer)
                .Include(x => x.Routine)
                .Include(x => x.Voucher)
                .Include(x => x.Shipment)
                .Include(x => x.Appointments)
                .ThenInclude(x => x.Service)
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(x => x.Status == status && x.OrderType == orderType)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            if (listOrders == null || !listOrders.Any())
            {
                return new HistoryBookingResponse
                {
                    data = new List<OrderModel>(),
                    pagination = new Pagination
                    {
                        page = page,
                        totalPage = 0,
                        totalCount = 0
                    }
                };
            }

            var totalCount = listOrders.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedOrders = listOrders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // 👉 Chuẩn: Lấy tất cả branchId cần thiết
            var branchIds = pagedOrders
                .SelectMany(x => x.OrderDetails)
                .Where(od => od.BranchId.HasValue)
                .Select(od => od.BranchId.Value)
                .Distinct()
                .ToList();

            var listBranches = await _unitOfWorks.BranchRepository
                .FindByCondition(b => branchIds.Contains(b.BranchId))
                .ToListAsync();

            var branchDict = listBranches.ToDictionary(b => b.BranchId, b => b);

            var orderModels = _mapper.Map<List<OrderModel>>(pagedOrders);

            // 👉 Map thêm Branch cho mỗi OrderDetail
            foreach (var order in orderModels)
            {
                var entityOrder = pagedOrders.FirstOrDefault(o => o.OrderId == order.OrderId);

                foreach (var detail in order.OrderDetails)
                {
                    if (detail.Product != null && detail.Branch == null && detail.BranchId.HasValue)
                    {
                        if (branchDict.TryGetValue(detail.BranchId.Value, out var matchedBranch))
                        {
                            detail.Branch = _mapper.Map<BranchModel>(matchedBranch);
                        }
                    }

                    // Optional: Reset lại Product.Branch nếu bạn không cần trả branch cho Product
                    if (detail.Product != null)
                    {
                        detail.Product.Branch = null;
                    }
                }
            }

            return new HistoryBookingResponse
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


        //public async Task<HistoryBookingResponse> BookingHistoryAllTypes(int userId, string status, int page = 1, int pageSize = 5)
        //{
        //    var listOrders = await _unitOfWorks.OrderRepository
        //        .FindByCondition(x => x.CustomerId == userId && x.Status == status)
        //        .Include(x => x.Customer)
        //        .Include(x => x.Routine)
        //        .Include(x => x.Voucher)
        //        .Include(x => x.Shipment)
        //        .Include(x => x.Appointments)
        //            .ThenInclude(x => x.Service)
        //        .Include(x => x.Appointments)
        //            .ThenInclude(x => x.Branch)
        //        .Include(x => x.OrderDetails)
        //            .ThenInclude(od => od.Promotion) // ✅ Bổ sung dòng này
        //        .Include(x => x.OrderDetails)
        //            .ThenInclude(od => od.Product)
        //                .ThenInclude(p => p.ProductImages)
        //        .Include(x => x.OrderDetails)
        //            .ThenInclude(od => od.Product)
        //                .ThenInclude(p => p.Branch_Products)
        //                    .ThenInclude(bp => bp.Branch)
        //        .OrderByDescending(x => x.CreatedDate)
        //        .ToListAsync();

        //    if (listOrders == null || !listOrders.Any())
        //    {
        //        return new HistoryBookingResponse()
        //        {
        //            data = new List<OrderModel>(),
        //            pagination = new Pagination
        //            {
        //                page = page,
        //                totalPage = 0,
        //                totalCount = 0
        //            }
        //        };
        //    }

        //    var totalCount = listOrders.Count();
        //    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        //    var pagedOrders = listOrders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        //    var orderModels = _mapper.Map<List<OrderModel>>(pagedOrders);

        //    // Gán branch và promotion thủ công nếu cần
        //    foreach (var order in orderModels)
        //    {
        //        var entityOrder = pagedOrders.FirstOrDefault(o => o.OrderId == order.OrderId);

        //        foreach (var detail in order.OrderDetails)
        //        {
        //            var entityDetail = entityOrder?.OrderDetails.FirstOrDefault(od => od.OrderDetailId == detail.OrderDetailId);

        //            // Gán Branch nếu thiếu
        //            if (detail.Product != null && detail.Branch == null)
        //            {
        //                var branchEntity = entityDetail?.Product?.Branch_Products?.FirstOrDefault()?.Branch;
        //                if (branchEntity != null)
        //                {
        //                    detail.Branch = _mapper.Map<BranchModel>(branchEntity);
        //                }
        //            }

        //            // Gán Promotion từ OrderDetail nếu có
        //            if (entityDetail?.Promotion != null)
        //            {
        //                detail.Promotion = _mapper.Map<PromotionModel>(entityDetail.Promotion);
        //            }

        //            // Chỉ gán Product.Promotion nếu chưa có OrderDetail.Promotion
        //            if (detail.Product != null && detail.Product.Promotion == null && entityDetail?.Promotion != null)
        //            {
        //                detail.Product.Promotion = _mapper.Map<PromotionDTO>(entityDetail.Promotion);
        //            }
        //            if (detail.Product != null)
        //            {
        //                detail.Product.Promotion = null;
        //                detail.Product.Branch = null;
        //            }
        //        }
        //    }


        //    return new HistoryBookingResponse()
        //    {
        //        message = "Lấy lịch sử tất cả loại đơn thành công!",
        //        data = orderModels,
        //        pagination = new Pagination
        //        {
        //            page = page,
        //            totalPage = totalPages,
        //            totalCount = totalCount
        //        }
        //    };
        //}

        public async Task<HistoryBookingResponse> BookingHistoryAllTypes(int userId, string status, int page = 1,
            int pageSize = 5)
        {
            var listOrders = await _unitOfWorks.OrderRepository
                .FindByCondition(x => x.CustomerId == userId && x.Status == status)
                .Include(x => x.Customer)
                .Include(x => x.Routine)
                .Include(x => x.Voucher)
                .Include(x => x.Shipment)
                .Include(x => x.Appointments)
                .ThenInclude(x => x.Service)
                .Include(x => x.Appointments)
                .ThenInclude(x => x.Branch)
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.Promotion)
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.ProductImages)
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.Branch) // ✅ thêm dòng này
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();


            if (listOrders == null || !listOrders.Any())
            {
                return new HistoryBookingResponse()
                {
                    data = new List<OrderModel>(),
                    pagination = new Pagination
                    {
                        page = page,
                        totalPage = 0,
                        totalCount = 0
                    }
                };
            }

            var totalCount = listOrders.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var pagedOrders = listOrders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // 👉 Bổ sung: Lấy tất cả Branch cần cho OrderDetails
            var branchIds = pagedOrders
                .SelectMany(o => o.OrderDetails)
                .Where(od => od.BranchId.HasValue)
                .Select(od => od.BranchId.Value)
                .Distinct()
                .ToList();

            var listBranches = await _unitOfWorks.BranchRepository
                .FindByCondition(b => branchIds.Contains(b.BranchId))
                .ToListAsync();

            var branchDict = listBranches.ToDictionary(b => b.BranchId, b => b);

            var orderModels = _mapper.Map<List<OrderModel>>(pagedOrders);

            // 👉 Map thêm Branch và Promotion
            foreach (var order in orderModels)
            {
                var entityOrder = pagedOrders.FirstOrDefault(o => o.OrderId == order.OrderId);

                foreach (var detail in order.OrderDetails)
                {
                    var entityDetail =
                        entityOrder?.OrderDetails.FirstOrDefault(od => od.OrderDetailId == detail.OrderDetailId);

                    // Gán Branch theo OrderDetail.BranchId
                    if (detail.Product != null && detail.Branch == null && detail.BranchId.HasValue)
                    {
                        if (branchDict.TryGetValue(detail.BranchId.Value, out var matchedBranch))
                        {
                            detail.Branch = _mapper.Map<BranchModel>(matchedBranch);
                        }
                    }

                    // Gán Promotion từ OrderDetail nếu có
                    if (entityDetail?.Promotion != null)
                    {
                        detail.Promotion = _mapper.Map<PromotionModel>(entityDetail.Promotion);
                    }

                    // Gán Promotion vào Product nếu chưa có
                    if (detail.Product != null)
                    {
                        if (detail.Product.Promotion == null && entityDetail?.Promotion != null)
                        {
                            detail.Product.Promotion = _mapper.Map<PromotionDTO>(entityDetail.Promotion);
                        }

                        // Xóa Branch và Promotion cũ trong Product
                        detail.Product.Branch = null;
                        detail.Product.Promotion = null;
                    }
                }
            }

            return new HistoryBookingResponse()
            {
                message = "Lấy lịch sử tất cả loại đơn thành công!",
                data = orderModels,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPages,
                    totalCount = totalCount
                }
            };
        }


        //public async Task<DetailOrderResponse> GetDetailOrder(int orderId, int userId)
        //{
        //    var user = await _unitOfWorks.UserRepository.GetByIdAsync(userId);
        //    if (user == null)
        //        return null;

        //    IQueryable<Order> query = _unitOfWorks.OrderRepository
        //        .FindByCondition(x => x.OrderId == orderId);

        //    if (user.RoleID == 3)
        //    {
        //        query = query.Where(x => x.CustomerId == userId);
        //    }

        //    var order = await query
        //        .Include(x => x.Customer)
        //        .Include(x => x.Shipment)
        //        .Include(x => x.Voucher)
        //        .Include(x => x.Routine)
        //        .Include(x => x.OrderDetails)
        //        .ThenInclude(od => od.Product)
        //        .ThenInclude(p => p.ProductImages)
        //        .Include(x => x.OrderDetails)
        //        .ThenInclude(od => od.Product)
        //        .ThenInclude(p => p.Branch_Products)
        //        .ThenInclude(bp => bp.Branch)
        //        .Include(x => x.Appointments)
        //        .FirstOrDefaultAsync();

        //    if (order == null)
        //    {
        //        return null; // Controller xử lý lỗi
        //    }

        //    var listService = new List<Data.Entities.Service>();
        //    var listProduct = new List<Product>();
        //    var listServiceModels = new List<ServiceModel>();
        //    var listProductModels = new List<ProductModel>();

        //    if (order.OrderType == OrderType.Appointment.ToString())
        //    {
        //        var orderAppointments = await _unitOfWorks.AppointmentsRepository
        //            .FindByCondition(x => x.OrderId == orderId)
        //            .Include(x => x.Branch)
        //            .Include(x => x.Service)
        //            .Include(x => x.Staff)
        //            .ThenInclude(x => x.StaffInfo)
        //            .ToListAsync();

        //        order.Appointments = orderAppointments;
        //        listService = orderAppointments.Select(a => a.Service).ToList();
        //        listServiceModels = await _serviceService.GetListImagesOfServices(listService);
        //    }
        //    else if (order.OrderType == OrderType.Product.ToString())
        //    {
        //        var orderDetails = order.OrderDetails.ToList();
        //        listProduct = orderDetails.Select(od => od.Product).ToList();
        //        listProductModels = await _productService.GetListImagesOfProduct(listProduct);
        //    }

        //    var orderModel = _mapper.Map<OrderModel>(order);

        //    if (orderModel.Appointments.Any())
        //    {
        //        foreach (var appointment in orderModel.Appointments)
        //        {
        //            var matchedService = listServiceModels.FirstOrDefault(s => s.ServiceId == appointment.ServiceId);
        //            if (matchedService != null)
        //            {
        //                appointment.Service.images = matchedService.images;
        //            }
        //        }
        //    }
        //    else if (orderModel.OrderDetails.Any())
        //    {
        //        foreach (var orderDetail in orderModel.OrderDetails)
        //        {
        //            var matchedProduct =
        //                listProductModels.FirstOrDefault(p => p.ProductId == orderDetail.Product.ProductId);
        //            if (matchedProduct != null)
        //            {
        //                orderDetail.Product.images = matchedProduct.images;
        //                orderDetail.Product.Branch = matchedProduct.Branch;
        //            }
        //        }
        //    }

        //    return new DetailOrderResponse
        //    {
        //        message = "Get detail order success",
        //        data = orderModel
        //    };
        //}

        //public async Task<DetailOrderResponse> GetDetailOrder(int orderId, int userId)
        //{
        //    var user = await _unitOfWorks.UserRepository.GetByIdAsync(userId);
        //    if (user == null)
        //        return null;

        //    IQueryable<Order> query = _unitOfWorks.OrderRepository
        //        .FindByCondition(x => x.OrderId == orderId);

        //    if (user.RoleID == 3)
        //    {
        //        query = query.Where(x => x.CustomerId == userId);
        //    }

        //    var order = await query
        //        .Include(x => x.Customer)
        //        .Include(x => x.Shipment)
        //        .Include(x => x.Voucher)
        //        .Include(x => x.Routine)
        //        .Include(x => x.OrderDetails)
        //            .ThenInclude(od => od.Product)
        //            .ThenInclude(p => p.ProductImages)
        //        .Include(x => x.Appointments)
        //        .FirstOrDefaultAsync();

        //    if (order == null)
        //    {
        //        return null;
        //    }

        //    var listServiceModels = new List<ServiceModel>();
        //    var listProductModels = new List<ProductModel>();
        //    var listBranches = new List<Branch>();

        //    if (order.OrderType == OrderType.Appointment.ToString())
        //    {
        //        var orderAppointments = await _unitOfWorks.AppointmentsRepository
        //            .FindByCondition(x => x.OrderId == orderId)
        //            .Include(x => x.Branch)
        //            .Include(x => x.Service)
        //            .Include(x => x.Staff)
        //                .ThenInclude(x => x.StaffInfo)
        //            .ToListAsync();

        //        order.Appointments = orderAppointments;

        //        var listService = orderAppointments.Select(a => a.Service).ToList();
        //        listServiceModels = await _serviceService.GetListImagesOfServices(listService);
        //    }
        //    else if (order.OrderType == OrderType.Product.ToString())
        //    {
        //        var orderDetails = order.OrderDetails.ToList();
        //        var listProducts = orderDetails.Select(od => od.Product).ToList();
        //        listProductModels = await _productService.GetListImagesOfProduct(listProducts);

        //        // 👉 Lấy branchId từ OrderDetail
        //        var branchIds = orderDetails
        //            .Where(od => od.BranchId.HasValue)
        //            .Select(od => od.BranchId.Value)
        //            .Distinct()
        //            .ToList();

        //        // 👉 Query tất cả Branch theo branchId
        //        listBranches = await _unitOfWorks.BranchRepository
        //            .FindByCondition(b => branchIds.Contains(b.BranchId))
        //            .ToListAsync();
        //    }

        //    var orderModel = _mapper.Map<OrderModel>(order);

        //    if (orderModel.OrderType == OrderType.Appointment.ToString())
        //    {
        //        if (orderModel.Appointments?.Any() == true)
        //        {
        //            foreach (var appointment in orderModel.Appointments)
        //            {
        //                var matchedService = listServiceModels.FirstOrDefault(s => s.ServiceId == appointment.ServiceId);
        //                if (matchedService != null)
        //                {
        //                    appointment.Service.images = matchedService.images;
        //                }
        //            }
        //        }
        //    }
        //    else if (orderModel.OrderType == OrderType.Product.ToString())
        //    {
        //        if (orderModel.OrderDetails?.Any() == true)
        //        {
        //            foreach (var orderDetail in orderModel.OrderDetails)
        //            {
        //                var matchedProduct = listProductModels.FirstOrDefault(p => p.ProductId == orderDetail.Product.ProductId);
        //                if (matchedProduct != null)
        //                {
        //                    orderDetail.Product.images = matchedProduct.images;
        //                }

        //                if (orderDetail.BranchId.HasValue)
        //                {
        //                    var matchedBranch = listBranches.FirstOrDefault(b => b.BranchId == orderDetail.BranchId.Value);
        //                    if (matchedBranch != null)
        //                    {
        //                        orderDetail.Product.Branch = new BranchDTO
        //                        {
        //                            BranchId = matchedBranch.BranchId,
        //                            BranchName = matchedBranch.BranchName,
        //                            BranchAddress = matchedBranch.BranchAddress,
        //                            BranchPhone = matchedBranch.BranchPhone,
        //                            LongAddress = matchedBranch.LongAddress,
        //                            LatAddress = matchedBranch.LatAddress,
        //                            Status = matchedBranch.Status,
        //                            ManagerId = matchedBranch.ManagerId,
        //                            District = matchedBranch.District,
        //                            WardCode = matchedBranch.WardCode,
        //                            CompanyId = matchedBranch.CompanyId,
        //                            CreatedDate = matchedBranch.CreatedDate,
        //                            UpdatedDate = matchedBranch.UpdatedDate,
        //                        };
        //                    }

        //                }
        //            }
        //        }
        //    }

        //    return new DetailOrderResponse
        //    {
        //        message = "Get detail order success",
        //        data = orderModel
        //    };
        //}

        //public async Task<DetailOrderResponse> GetDetailOrder(int orderId, int userId)
        //{
        //    var user = await _unitOfWorks.UserRepository.GetByIdAsync(userId);
        //    if (user == null)
        //        return null;

        //    IQueryable<Order> query = _unitOfWorks.OrderRepository
        //        .FindByCondition(x => x.OrderId == orderId);

        //    if (user.RoleID == 3)
        //    {
        //        query = query.Where(x => x.CustomerId == userId);
        //    }

        //    var order = await query
        //        .Include(x => x.Customer)
        //        .Include(x => x.Shipment)
        //        .Include(x => x.Voucher)
        //        .Include(x => x.Routine)
        //        .Include(x => x.OrderDetails)
        //        .ThenInclude(od => od.Product)
        //        .ThenInclude(p => p.ProductImages)
        //        .Include(x => x.Appointments)
        //        .FirstOrDefaultAsync();

        //    if (order == null)
        //    {
        //        return null;
        //    }

        //    var listServiceModels = new List<ServiceModel>();
        //    var listProductModels = new List<ProductModel>();
        //    var listBranches = new List<Branch>();

        //    // ✅ Nếu là Appointment hoặc ProductAndService: lấy Appointments
        //    if (order.OrderType == OrderType.Appointment.ToString() ||
        //        order.OrderType == OrderType.ProductAndService.ToString())
        //    {
        //        var orderAppointments = await _unitOfWorks.AppointmentsRepository
        //            .FindByCondition(x => x.OrderId == orderId)
        //            .Include(x => x.Branch)
        //            .Include(x => x.Service)
        //            .Include(x => x.Staff)
        //            .ThenInclude(x => x.StaffInfo)
        //            .ToListAsync();

        //        order.Appointments = orderAppointments;

        //        var listService = orderAppointments.Select(a => a.Service).ToList();
        //        listServiceModels = await _serviceService.GetListImagesOfServices(listService);
        //    }

        //    // ✅ Nếu là Product hoặc ProductAndService: lấy OrderDetails
        //    if (order.OrderType == OrderType.Product.ToString() ||
        //        order.OrderType == OrderType.ProductAndService.ToString())
        //    {
        //        var orderDetails = order.OrderDetails.ToList();
        //        var listProducts = orderDetails.Select(od => od.Product).ToList();
        //        listProductModels = await _productService.GetListImagesOfProduct(listProducts);

        //        var branchIds = orderDetails
        //            .Where(od => od.BranchId.HasValue)
        //            .Select(od => od.BranchId.Value)
        //            .Distinct()
        //            .ToList();

        //        listBranches = await _unitOfWorks.BranchRepository
        //            .FindByCondition(b => branchIds.Contains(b.BranchId))
        //            .ToListAsync();
        //    }

        //    var orderModel = _mapper.Map<OrderModel>(order);

        //    // 👉 Map Services nếu có
        //    if (orderModel.Appointments?.Any() == true)
        //    {
        //        foreach (var appointment in orderModel.Appointments)
        //        {
        //            var matchedService = listServiceModels.FirstOrDefault(s => s.ServiceId == appointment.ServiceId);
        //            if (matchedService != null)
        //            {
        //                appointment.Service.images = matchedService.images;
        //            }
        //        }
        //    }

        //    // 👉 Map Products nếu có
        //    if (orderModel.OrderDetails?.Any() == true)
        //    {
        //        foreach (var orderDetail in orderModel.OrderDetails)
        //        {
        //            var matchedProduct =
        //                listProductModels.FirstOrDefault(p => p.ProductId == orderDetail.Product.ProductId);
        //            if (matchedProduct != null)
        //            {
        //                orderDetail.Product.images = matchedProduct.images;
        //            }

        //            if (orderDetail.BranchId.HasValue)
        //            {
        //                var matchedBranch = listBranches.FirstOrDefault(b => b.BranchId == orderDetail.BranchId.Value);
        //                if (matchedBranch != null)
        //                {
        //                    orderDetail.Product.Branch = new BranchDTO
        //                    {
        //                        BranchId = matchedBranch.BranchId,
        //                        BranchName = matchedBranch.BranchName,
        //                        BranchAddress = matchedBranch.BranchAddress,
        //                        BranchPhone = matchedBranch.BranchPhone,
        //                        LongAddress = matchedBranch.LongAddress,
        //                        LatAddress = matchedBranch.LatAddress,
        //                        Status = matchedBranch.Status,
        //                        ManagerId = matchedBranch.ManagerId,
        //                        District = matchedBranch.District,
        //                        WardCode = matchedBranch.WardCode,
        //                        CompanyId = matchedBranch.CompanyId,
        //                        CreatedDate = matchedBranch.CreatedDate,
        //                        UpdatedDate = matchedBranch.UpdatedDate,
        //                    };
        //                }
        //            }
        //        }
        //    }

        //    return new DetailOrderResponse
        //    {
        //        message = "Get detail order success",
        //        data = orderModel
        //    };
        //}

        public async Task<DetailOrderResponse> GetDetailOrder(int orderId, int userId)
        {
            var user = await _unitOfWorks.UserRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            IQueryable<Order> query = _unitOfWorks.OrderRepository
                .FindByCondition(x => x.OrderId == orderId);

            if (user.RoleID == 3)
            {
                query = query.Where(x => x.CustomerId == userId);
            }

            var order = await query
                .Include(x => x.Customer)
                .Include(x => x.Shipment)
                .Include(x => x.Voucher)
                .Include(x => x.Routine)
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.ProductImages)
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.Branch) // ✅ Include thêm Branch
                .FirstOrDefaultAsync();

            if (order == null)
                return null;

            // Lấy Appointments
            var orderAppointments = await _unitOfWorks.AppointmentsRepository
                .FindByCondition(x => x.OrderId == orderId)
                .Include(x => x.Branch)
                .Include(x => x.Service)
                .Include(x => x.Staff)
                .ThenInclude(s => s.StaffInfo)
                .Include(x => x.Customer)
                .ToListAsync();

            order.Appointments = orderAppointments;

            var listServiceModels = new List<ServiceModel>();
            if (orderAppointments.Any())
            {
                var listServices = orderAppointments
                    .Where(a => a.Service != null)
                    .Select(a => a.Service)
                    .ToList();

                listServiceModels = await _serviceService.GetListImagesOfServices(listServices);
            }

            var listProductModels = new List<ProductModel>();
            if (order.OrderType == OrderType.Product.ToString() ||
                order.OrderType == OrderType.ProductAndService.ToString())
            {
                var listProducts = order.OrderDetails.Select(od => od.Product).ToList();
                listProductModels = await _productService.GetListImagesOfProduct(listProducts);
            }

            var orderModel = _mapper.Map<OrderModel>(order);

            // Gán images cho service
            foreach (var appointment in orderModel.Appointments ?? [])
            {
                var matchedService = listServiceModels.FirstOrDefault(s => s.ServiceId == appointment.ServiceId);
                if (matchedService != null)
                {
                    appointment.Service.images = matchedService.images;
                }
            }

            // Gán images và branch cho sản phẩm
            foreach (var orderDetail in orderModel.OrderDetails ?? [])
            {
                var matchedProduct =
                    listProductModels.FirstOrDefault(p => p.ProductId == orderDetail.Product?.ProductId);
                if (matchedProduct != null)
                {
                    orderDetail.Product.images = matchedProduct.images;
                }

                // Gán Branch nếu chưa có
                if (orderDetail.Branch == null && orderDetail.BranchId.HasValue)
                {
                    var matchedEntity = order.OrderDetails
                        .FirstOrDefault(od => od.OrderDetailId == orderDetail.OrderDetailId);

                    if (matchedEntity?.Branch != null)
                    {
                        orderDetail.Branch = _mapper.Map<BranchModel>(matchedEntity.Branch);
                    }
                }

                // ❌ Xóa branch trong Product để tránh lặp
                if (orderDetail.Product != null)
                {
                    orderDetail.Product.Branch = null;
                }
            }


            return new DetailOrderResponse
            {
                message = "Get detail order success",
                data = orderModel
            };
        }


        public async Task<List<AppointmentsModel>> CreateMoreOrderAppointment(int userId,
            AppointmentCreateMoreRequest request,
            int existingOrderId)
        {
            await _unitOfWorks.BeginTransactionAsync();

            try
            {
                var customer = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
                var branch =
                    await _unitOfWorks.BranchRepository.FirstOrDefaultAsync(x => x.BranchId == request.BranchId);
                Voucher voucher = null;

                // Kiểm tra voucher (nếu có)
                if (request.VoucherId != null && request.VoucherId > 0)
                {
                    voucher = await _unitOfWorks.VoucherRepository
                                  .FirstOrDefaultAsync(x => x.VoucherId == request.VoucherId)
                              ?? throw new BadRequestException("Không tìm thấy voucher!");
                }

                if (customer == null) throw new BadRequestException("Không tìm thấy thông tin khách hàng!");
                if (branch == null) throw new BadRequestException("Không tìm thấy thông tin chi nhánh!");

                // Kiểm tra tính hợp lệ của số lượng dịch vụ, nhân viên và thời gian
                if (request.ServiceId.Length != request.StaffId.Length ||
                    request.ServiceId.Length != request.AppointmentsTime.Length)
                {
                    throw new BadRequestException("The number of services, staff, and appointment times must match!");
                }

                // Lấy thông tin Order đã tồn tại
                var existingOrder =
                    await _unitOfWorks.OrderRepository.FirstOrDefaultAsync(x => x.OrderId == existingOrderId);
                if (existingOrder == null)
                {
                    throw new BadRequestException("Không tìm thấy đơn hàng!");
                }

                if (existingOrder.OrderType == OrderType.Product.ToString() ||
                    existingOrder.OrderType == OrderType.Routine.ToString())
                {
                    throw new BadRequestException("Order type không phù hợp!");
                }

                // Kiểm tra trạng thái của order
                if (existingOrder.Status == OrderStatusEnum.Completed.ToString() ||
                    existingOrder.Status == OrderStatusEnum.Cancelled.ToString())
                {
                    throw new BadRequestException("Không thể thêm cuộc hẹn vào đơn hàng đã hoàn thành hoặc bị hủy!");
                }

                // Đảm bảo rằng order vẫn đang trong trạng thái chờ xử lý
                var randomOrderCode = new Random().Next(100000, 999999);
                existingOrder.OrderCode = randomOrderCode;
                existingOrder.UpdatedDate = DateTime.UtcNow;

                _unitOfWorks.OrderRepository.Update(existingOrder);
                await _unitOfWorks.OrderRepository.Commit();

                // Nếu voucher có, giảm số lượng voucher
                if (voucher != null)
                {
                    voucher.RemainQuantity -= 1;
                    if (voucher.RemainQuantity < 0)
                    {
                        throw new BadRequestException("Voucher đã hết hạn!");
                    }

                    existingOrder.DiscountAmount = voucher.DiscountAmount;
                    existingOrder.VoucherId = voucher.VoucherId;
                    _unitOfWorks.OrderRepository.Update(existingOrder);
                    await _unitOfWorks.OrderRepository.Commit();

                    _unitOfWorks.VoucherRepository.Update(voucher);
                    await _unitOfWorks.VoucherRepository.Commit();
                }

                var appointments = new List<AppointmentsModel>();
                var staffAppointments =
                    new Dictionary<int, DateTime>(); // Lưu lịch làm việc của nhân viên trong request

                // Duyệt qua từng dịch vụ, nhân viên và thời gian
                for (int i = 0; i < request.ServiceId.Length; i++)
                {
                    var serviceId = request.ServiceId[i];
                    var staffId = request.StaffId[i];
                    var appointmentTime = request.AppointmentsTime[i];

                    if (appointmentTime < DateTime.Now)
                    {
                        throw new BadRequestException($"Thời gian đặt lịch không hợp lệ: {appointmentTime}");
                    }

                    var service =
                        await _unitOfWorks.ServiceRepository.FirstOrDefaultAsync(x => x.ServiceId == serviceId);
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

                    var newAppointment = new AppointmentsModel
                    {
                        CustomerId = userId,
                        OrderId = existingOrder.OrderId, // Sử dụng OrderId hiện tại
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

                    // Handle notifications and MongoDB interactions
                    var specialistMySQL = await _staffService.GetStaffById(staffId);
                    var adminMongo = await _mongoDbService.GetCustomerByIdAsync(branch.ManagerId);
                    var specialistMongo = await _mongoDbService.GetCustomerByIdAsync(specialistMySQL.StaffInfo.UserId);
                    var customerMongo = await _mongoDbService.GetCustomerByIdAsync(customer.UserId);

                    var channel = await _mongoDbService.CreateChannelAsync(
                        $"Channel {appointmentEntity.AppointmentId} {service.Name}", adminMongo!.Id,
                        appointmentEntity.AppointmentId);

                    await _mongoDbService.AddMemberToChannelAsync(channel.Id, specialistMongo!.Id);
                    await _mongoDbService.AddMemberToChannelAsync(channel.Id, customerMongo!.Id);

                    var userMongo = await _mongoDbService.GetCustomerByIdAsync(customer.UserId)
                                    ?? throw new BadRequestException(
                                        "Không tìm thấy thông tin khách hàng trong MongoDB!");

                    // create notification
                    var notification = new Notifications()
                    {
                        UserId = customer.UserId,
                        Content =
                            $"Bạn có cuộc hẹn mới với {staff.StaffInfo.FullName} vào lúc {newAppointment.AppointmentsTime}",
                        Type = "Appointment",
                        isRead = false,
                        ObjectId = appointmentEntity.AppointmentId,
                        CreatedDate = DateTime.UtcNow,
                    };

                    await _unitOfWorks.NotificationRepository.AddAsync(notification);
                    await _unitOfWorks.NotificationRepository.Commit();

                    if (NotificationHub.TryGetConnectionId(userMongo.Id, out var connectionId))
                    {
                        _logger.LogInformation("User connected: {userId} => {connectionId}", userMongo.Id,
                            connectionId);
                        Console.WriteLine($"User connected: {userMongo.Id} => {connectionId}");
                        await _hubContext.Clients.Client(connectionId).SendAsync("receiveNotification", notification);
                    }

                    if (NotificationHub.TryGetConnectionId(specialistMongo.Id, out var connectionSpecialListId))
                    {
                        _logger.LogInformation("User connected: {userId} => {connectionId}", specialistMongo.Id,
                            connectionId);
                        Console.WriteLine($"User connected: {specialistMongo.Id} => {connectionSpecialListId}");
                        await _hubContext.Clients.Client(connectionSpecialListId)
                            .SendAsync("receiveNotification", notification);
                    }
                }

                // Cập nhật tổng giá trị đơn hàng
                existingOrder.TotalAmount = appointments.Sum(a => a.SubTotal);
                _unitOfWorks.OrderRepository.Update(existingOrder);
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

        public async Task<bool> CreateMoreOrderProduct(int orderId, OrderDetailRequest request)
        {
            // Bắt đầu giao dịch
            await _unitOfWorks.BeginTransactionAsync();

            try
            {
                var order = await _unitOfWorks.OrderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new BadRequestException("Không tìm thấy đơn hàng!");
                }

                if (order.OrderType == OrderType.Appointment.ToString() ||
                    order.OrderType == OrderType.Routine.ToString())
                {
                    throw new BadRequestException("Order type không phù hợp!");
                }

                // Kiểm tra trạng thái của order
                if (order.Status == OrderStatusEnum.Completed.ToString() ||
                    order.Status == OrderStatusEnum.Cancelled.ToString())
                {
                    throw new BadRequestException("Không thể thêm cuộc hẹn vào đơn hàng đã hoàn thành hoặc bị hủy!");
                }

                if (request.ProductIds == null || request.ProductIds.Length == 0)
                {
                    throw new BadRequestException("At least one ProductId is required!");
                }

                // Kiểm tra rằng số lượng sản phẩm có đủ cho mỗi ProductId
                if (request.ProductIds.Length != request.Quantity.Length)
                {
                    throw new BadRequestException("Number of ProductIds must match number of Quantities.");
                }

                var branch = await _unitOfWorks.BranchRepository
                                 .FirstOrDefaultAsync(x => x.BranchId == request.BranchId)
                             ?? throw new BadRequestException("Không tìm thấy thông tin chi nhánh");

                // Duyệt qua từng sản phẩm và xử lý
                for (int i = 0; i < request.ProductIds.Length; i++)
                {
                    var productId = request.ProductIds[i];
                    var quantity = request.Quantity[i]; // Lấy số lượng tương ứng với ProductId

                    var product = await _unitOfWorks.ProductRepository.GetByIdAsync(productId);

                    var productBranch = await _unitOfWorks.Branch_ProductRepository
                                            .FindByCondition(x =>
                                                x.ProductId == productId && x.BranchId == branch.BranchId)
                                            .FirstOrDefaultAsync()
                                        ?? throw new BadRequestException(
                                            "Không tìm thấy thông tin sản phẩm trong chi nhánh");

                    if (product == null)
                    {
                        throw new BadRequestException($"Sản phẩm với ID {productId} không tồn tại!");
                    }

                    var productPrice = product?.Price ?? 0;

                    var orderDetail = new OrderDetail
                    {
                        OrderId = orderId,
                        ProductId = productBranch.ProductId,
                        BranchId = productBranch.BranchId,
                        PromotionId = request.PromotionId > 0 ? request.PromotionId : null,
                        Quantity = quantity, // Gán số lượng đúng cho sản phẩm
                        UnitPrice = productPrice,
                        SubTotal = quantity * productPrice, // Tính tổng số tiền cho sản phẩm
                        Status = request.Status ?? OrderStatusEnum.Pending.ToString(),
                        StatusPayment = request.StatusPayment ?? OrderStatusPaymentEnum.Pending.ToString(),
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    await _unitOfWorks.OrderDetailRepository.AddAsync(orderDetail);
                }

                // Commit giao dịch
                var result = await _unitOfWorks.OrderDetailRepository.Commit();
                if (result > 0)
                {
                    // Nếu commit thành công, thực hiện commit giao dịch tổng
                    await _unitOfWorks.CommitTransactionAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                // Nếu có lỗi, rollback giao dịch
                await _unitOfWorks.RollbackTransactionAsync();
                throw;
            }
        }
        
        
        public async Task<bool> UpdateOrderStatus(int orderId, string orderStatus)
        {
            var order = await _unitOfWorks.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new BadRequestException("Order not found!");

            if (order.Status == OrderStatusEnum.Completed.ToString())
                throw new BadRequestException("Đơn hàng đã hoàn thành không thể thay đổi trạng thái!");

            var customer = await _unitOfWorks.UserRepository.GetByIdAsync(order.CustomerId);
            if (customer == null)
                throw new BadRequestException("Customer not found!");

            if (orderStatus == OrderStatusEnum.Completed.ToString())
            {
                customer.BonusPoint += 200;
                _unitOfWorks.UserRepository.Update(customer);
                await _unitOfWorks.UserRepository.Commit();
            }

            order.Status = orderStatus;
            order.UpdatedDate = DateTime.Now;
            _unitOfWorks.OrderRepository.Update(order);
            var result = await _unitOfWorks.OrderRepository.Commit() > 0;

            // Gửi notification
            var notification = new Notifications()
            {
                UserId = customer.UserId,
                Content = $"Trạng thái đơn hàng #{order.OrderId} đã được cập nhật thành: {orderStatus}",
                Type = "Order",
                isRead = false,
                ObjectId = order.OrderId,
                CreatedDate = DateTime.UtcNow,
            };

            await _unitOfWorks.NotificationRepository.AddAsync(notification);
            await _unitOfWorks.NotificationRepository.Commit();

            // Gửi real-time notification nếu đang kết nối
            var userMongo = await _mongoDbService.GetCustomerByIdAsync(customer.UserId); // tùy theo hệ thống của bạn
            if (NotificationHub.TryGetConnectionId(userMongo.Id, out var connectionId))
            {
                _logger.LogInformation("User connected: {userId} => {connectionId}", userMongo.Id, connectionId);
                await _hubContext.Clients.Client(connectionId).SendAsync("receiveNotification", notification);
            }

            return result;
        }


        public async Task<bool> CancelOrder(int orderId, string reasonCancel)
        {
            var order = await _unitOfWorks.OrderRepository.GetByIdAsync(orderId);
            if (order == null) throw new BadRequestException("Order not found!");
            order.Status = OrderStatusEnum.Cancelled.ToString();
            order.Note = reasonCancel;
            order.UpdatedDate = DateTime.Now;
            _unitOfWorks.OrderRepository.Update(order);
            return await _unitOfWorks.OrderRepository.Commit() > 0;
        }

        public async Task<GetListOrderFilterResponse> GetListOrderFilterAsync(GetAllOrderRequest request)
        {
            IQueryable<Order> query = _unitOfWorks.OrderRepository.FindByCondition(x =>
                    (string.IsNullOrEmpty(request.OrderType) || x.OrderType == request.OrderType) &&
                    (string.IsNullOrEmpty(request.OrderStatus) || x.Status == request.OrderStatus) &&
                    (string.IsNullOrEmpty(request.PaymentStatus) || x.StatusPayment == request.PaymentStatus)
                )
                .Include(x => x.Customer)
                .Include(x => x.Shipment)
                .Include(x => x.Voucher)
                .Include(x => x.Routine)
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.ProductImages)
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.Branch_Products)
                .ThenInclude(bp => bp.Branch)
                .Include(x => x.Appointments)
                .ThenInclude(x => x.Service)
                .ThenInclude(x => x.ServiceCategory)
                .OrderByDescending(x => x.CreatedDate);

            if (request.BranchId.HasValue)
            {
                query = query.Where(order =>
                    (order.OrderType == OrderType.Appointment.ToString() ||
                     order.OrderType == OrderType.Routine.ToString() ||
                     order.OrderType == OrderType.ProductAndService.ToString()) &&
                    order.Appointments.Any(a => a.BranchId == request.BranchId.Value)
                    ||
                    (order.OrderType == OrderType.Product.ToString() ||
                     order.OrderType == OrderType.Routine.ToString() ||
                     order.OrderType == OrderType.ProductAndService.ToString()) &&
                    order.OrderDetails.Any(od =>
                        od.Product.Branch_Products.Any(bp => bp.BranchId == request.BranchId.Value))
                );
            }


            var pageSize = request.PageSize ?? 10;
            var pageIndex = request.PageIndex ?? 1;

            var totalItemsCount = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = _mapper.Map<List<OrderModel>>(items);

            // Gán thêm images
            foreach (var order in result)
            {
                var hasService = order.OrderType == OrderType.Appointment.ToString() ||
                                 order.OrderType == OrderType.Routine.ToString() ||
                                 order.OrderType == OrderType.ProductAndService.ToString();
                var hasProduct = order.OrderType == OrderType.Product.ToString() ||
                                 order.OrderType == OrderType.Routine.ToString() ||
                                 order.OrderType == OrderType.ProductAndService.ToString();

                if (hasService)
                {
                    var services = order.Appointments.Select(a => a.Service).ToList();
                    var serviceModels = await _serviceService.GetListImagesOfServices(
                        _mapper.Map<List<Data.Entities.Service>>(services));
                    foreach (var appointment in order.Appointments)
                    {
                        var matchedService = serviceModels.FirstOrDefault(s => s.ServiceId == appointment.ServiceId);
                        if (matchedService != null)
                            appointment.Service.images = matchedService.images;
                    }
                }

                if (hasProduct)
                {
                    var products = order.OrderDetails.Select(od => od.Product).ToList();
                    var productModels =
                        await _productService.GetListImagesOfProduct(_mapper.Map<List<Product>>(products));
                    foreach (var detail in order.OrderDetails)
                    {
                        var matchedProduct = productModels.FirstOrDefault(p => p.ProductId == detail.Product.ProductId);
                        if (matchedProduct != null)
                        {
                            detail.Product.images = matchedProduct.images;
                            detail.Product.Branch = matchedProduct.Branch;
                        }
                    }
                }
            }

            return new GetListOrderFilterResponse
            {
                message = "Lấy danh sách đơn hàng thành công!",
                data = result,
                pagination = new Pagination
                {
                    page = pageIndex,
                    totalPage = (int)Math.Ceiling(totalItemsCount / (double)pageSize),
                    totalCount = totalItemsCount
                }
            };
        }


        public async Task<Pagination<AppointmentsModel>> GetListAppointmentsByStaff(string status, int staffId,
            int pageIndex = 1, int pageSize = 10)
        {
            var staff = await _unitOfWorks.StaffRepository.FindByCondition(x => x.UserId == staffId)
                .FirstOrDefaultAsync();
            var query = _unitOfWorks.AppointmentsRepository.FindByCondition(x =>
                x.StaffId == staff.StaffId && x.Status == status);
            var totalItemsCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItemsCount / (double)pageSize);
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new Pagination<AppointmentsModel>
            {
                TotalItemsCount = totalItemsCount,
                PageSize = pageSize,
                PageIndex = pageIndex,
                Data = _mapper.Map<List<AppointmentsModel>>(items)
            };
        }

        private bool CanModifyOrder(DateTime createdDate, int allowedHours = 24)
        {
            var timeElapsed = DateTime.UtcNow - createdDate;
            return timeElapsed.TotalHours <= allowedHours;
        }


        public async Task<ApiResult<ApiResponse>> CreateOrderWithDetailsAsync(CreateOrderWithDetailsRequest request)
        {
            if (request == null
                || request.Products == null
                || !request.Products.Any()
                || string.IsNullOrWhiteSpace(request.PaymentMethod))
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Vui lòng nhập đầy đủ thông tin đơn hàng."
                });
            }

            using var transaction = await _unitOfWorks.BeginTransactionAsync();

            try
            {
                var user = await _unitOfWorks.UserRepository
                    .FindByCondition(x => x.UserId == request.UserId && x.Status == "Active")
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "User không tồn tại hoặc đang bị vô hiệu hóa."
                    });
                }

                int? voucherId = null;
                if (request.VoucherId.HasValue)
                {
                    var voucher = await _unitOfWorks.VoucherRepository
                        .FindByCondition(x => x.VoucherId == request.VoucherId && x.Status == "Active")
                        .FirstOrDefaultAsync();

                    if (voucher == null)
                    {
                        return ApiResult<ApiResponse>.Error(new ApiResponse
                        {
                            message = "Mã giảm giá không hợp lệ."
                        });
                    }

                    voucherId = voucher.VoucherId;
                }

                int orderCode;
                var random = new Random();
                do
                {
                    orderCode = random.Next(1000, 10000);
                } while (await _unitOfWorks.OrderRepository
                             .FindByCondition(x => x.OrderCode == orderCode)
                             .AnyAsync());

                var order = new Order
                {
                    OrderCode = orderCode,
                    CustomerId = request.UserId,
                    VoucherId = voucherId,
                    TotalAmount = 0,
                    OrderType = "Product",
                    PaymentMethod = request.PaymentMethod,
                    Status = OrderStatusEnum.Pending.ToString(),
                    StatusPayment = OrderStatusPaymentEnum.Pending.ToString(),
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                await _unitOfWorks.OrderRepository.AddAsync(order);
                await _unitOfWorks.OrderRepository.Commit();

                decimal calculatedTotal = 0;

                foreach (var item in request.Products)
                {
                    if (item.Quantity <= 0)
                        throw new BadRequestException(
                            $"Số lượng sản phẩm [ProductBranchId = {item.ProductBranchId}] phải lớn hơn 0.");

                    var branchProduct = await _unitOfWorks.Branch_ProductRepository
                        .FindByCondition(bp => bp.Id == item.ProductBranchId && bp.Status == "Active")
                        .Include(bp => bp.Promotion)
                        .FirstOrDefaultAsync();

                    if (branchProduct == null)
                        throw new BadRequestException($"Không tìm thấy BranchProduct với ID = {item.ProductBranchId}.");

                    if (branchProduct.StockQuantity < item.Quantity)
                        throw new BadRequestException($"Sản phẩm ID {branchProduct.ProductId} không đủ tồn kho.");

                    var product = await _unitOfWorks.ProductRepository.GetByIdAsync(branchProduct.ProductId);
                    if (product == null)
                        throw new BadRequestException($"Không tìm thấy Product với ID = {branchProduct.ProductId}.");

                    decimal unitPrice = product.Price;
                    decimal discountAmount = 0;
                    decimal subTotal;

                    // Áp dụng giảm giá nếu có promotion hợp lệ
                    if (branchProduct.Promotion != null &&
                        branchProduct.Promotion.Status == "Active" &&
                        branchProduct.Promotion.StartDate <= DateTime.Now &&
                        branchProduct.Promotion.EndDate >= DateTime.Now)
                    {
                        discountAmount = unitPrice * item.Quantity * (branchProduct.Promotion.DiscountPercent / 100M);
                    }

                    subTotal = (unitPrice * item.Quantity) - discountAmount;
                    calculatedTotal += subTotal;

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = product.ProductId,
                        BranchId = branchProduct.BranchId,
                        PromotionId = branchProduct.PromotionId,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice,
                        SubTotal = subTotal,
                        DiscountAmount = discountAmount,
                        Status = OrderStatusEnum.Pending.ToString(),
                        StatusPayment = OrderStatusPaymentEnum.Pending.ToString(),
                        PaymentMethod = request.PaymentMethod,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    await _unitOfWorks.OrderDetailRepository.AddAsync(orderDetail);

                    branchProduct.StockQuantity -= item.Quantity;
                    branchProduct.UpdatedDate = DateTime.Now;
                    _unitOfWorks.Branch_ProductRepository.Update(branchProduct);
                }

                // Gán thông tin người nhận
                var recipientName = !string.IsNullOrWhiteSpace(request.RecipientName)
                    ? request.RecipientName
                    : (!string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : "");

                var recipientAddress = !string.IsNullOrWhiteSpace(request.RecipientAddress)
                    ? request.RecipientAddress
                    : (!string.IsNullOrWhiteSpace(user.Address) ? user.Address : "");

                var recipientPhone = !string.IsNullOrWhiteSpace(request.RecipientPhone)
                    ? request.RecipientPhone
                    : (!string.IsNullOrWhiteSpace(user.PhoneNumber) ? user.PhoneNumber : "");

                var shipment = new Shipment
                {
                    OrderId = order.OrderId,
                    EstimatedDeliveryDate = request.EstimatedDeliveryDate,
                    ShippingCost = request.ShippingCost ?? 0,
                    RecipientName = recipientName,
                    RecipientAddress = recipientAddress,
                    RecipientPhone = recipientPhone,
                    ShippingStatus = ShippingStatusEnum.Pending.ToString(),
                    ShippingCarrier = "Unknown",
                    TrackingNumber = "Unknown",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _unitOfWorks.ShipmentRepository.AddAsync(shipment);
                calculatedTotal += shipment.ShippingCost;

                order.TotalAmount = calculatedTotal;
                order.UpdatedDate = DateTime.Now;
                _unitOfWorks.OrderRepository.Update(order);

                // Commit các thay đổi
                await _unitOfWorks.OrderRepository.Commit();
                await _unitOfWorks.OrderDetailRepository.Commit();
                await _unitOfWorks.ShipmentRepository.Commit();
                await _unitOfWorks.Branch_ProductRepository.Commit();
                await transaction.CommitAsync();

                return ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Tạo đơn hàng thành công.",
                    data = order.OrderId
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi tạo đơn hàng: {ex.Message}"
                });
            }
        }


        public async Task<ApiResult<object>> UpdateOrderStatusSimpleAsync(int orderId, string status, string token)
        {
            var currentUser = await _authService.GetUserInToken(token);
            if (currentUser == null)
            {
                return ApiResult<object>.Error(ApiResponse.Error("Invalid token or user not found."));
            }

            int? staffRoleId = null;

            if (currentUser.RoleID == 4) // Nhân viên
            {
                var staff = await _unitOfWorks.StaffRepository
                    .FindByCondition(x => x.UserId == currentUser.UserId)
                    .FirstOrDefaultAsync();

                staffRoleId = staff?.RoleId;
            }

            var order = await _unitOfWorks.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return ApiResult<object>.Error(ApiResponse.Error("Order not found."));
            }

            string currentStatus = order.Status;
            string newStatus = status;

            // Validate chuyển trạng thái theo role
            if (currentUser.RoleID == 3) // Customer
            {
                if (newStatus != OrderStatusEnum.Completed.ToString() &&
                    newStatus != OrderStatusEnum.Cancelled.ToString())
                {
                    return ApiResult<object>.Error(
                        ApiResponse.Error(
                            "Khách hàng chỉ có thể cập nhật trạng thái thành 'Completed' hoặc 'Cancelled'."));
                }

                if (newStatus == OrderStatusEnum.Completed.ToString() &&
                    currentStatus != OrderStatusEnum.Shipping.ToString())
                {
                    return ApiResult<object>.Error(
                        ApiResponse.Error("Chỉ có thể hoàn thành đơn hàng đang ở trạng thái 'Shipping'."));
                }

                if (newStatus == OrderStatusEnum.Cancelled.ToString() &&
                    currentStatus != OrderStatusEnum.Pending.ToString())
                {
                    return ApiResult<object>.Error(
                        ApiResponse.Error("Chỉ có thể hủy đơn hàng đang ở trạng thái 'Pending'."));
                }
            }
            else if (currentUser.RoleID == 4 && staffRoleId == 1) // Cashier
            {
                if (newStatus != OrderStatusEnum.Shipping.ToString())
                {
                    return ApiResult<object>.Error(
                        ApiResponse.Error("Thu ngân chỉ được cập nhật đơn hàng sang trạng thái 'Shipping'."));
                }

                if (currentStatus != OrderStatusEnum.Pending.ToString())
                {
                    return ApiResult<object>.Error(
                        ApiResponse.Error("Chỉ có thể chuyển trạng thái từ 'Pending' sang 'Shipping'."));
                }
            }
            else
            {
                return ApiResult<object>.Error(ApiResponse.Error("Bạn không được quyền thực hiện."));
            }

            if (currentStatus == newStatus)
            {
                return ApiResult<object>.Error(ApiResponse.Error("Trạng thái đơn hàng đã là trạng thái này."));
            }

            order.Status = newStatus;
            order.UpdatedDate = DateTime.Now;
            _unitOfWorks.OrderRepository.Update(order);
            await _unitOfWorks.OrderRepository.Commit();

            var response = ApiResponse.Succeed(new
            {
                OrderId = order.OrderId,
                NewStatus = order.Status
            }, "Order status updated successfully.");

            return ApiResult<object>.Succeed(response);
        }


        public async Task<ApiResult<object>> UpdatePaymentMethodOrNoteAsync(UpdatePaymentMethodOrNoteRequest request)
        {
            // Lấy thông tin đơn hàng
            var order = await _unitOfWorks.OrderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return ApiResult<object>.Error(null, "Không tìm thấy đơn hàng.");
            }


            // Kiểm tra trạng thái đơn hàng
            if (order.Status != OrderStatusEnum.Pending.ToString())
            {
                return ApiResult<object>.Error(null, "Chỉ có thể cập nhật khi đơn hàng có trạng thái 'Pending'.");
            }

            // Cập nhật phương thức thanh toán hoặc ghi chú
            if (!string.IsNullOrEmpty(request.PaymentMethod))
            {
                order.PaymentMethod = request.PaymentMethod;
            }

            if (!string.IsNullOrEmpty(request.Note))
            {
                order.Note = request.Note;
            }

            // Cập nhật thời gian thay đổi
            order.UpdatedDate = DateTime.UtcNow;

            // Lưu lại thay đổi vào cơ sở dữ liệu
            _unitOfWorks.OrderRepository.Update(order);
            await _unitOfWorks.OrderRepository.Commit();

            return ApiResult<object>.Succeed(new
            {
                OrderId = order.OrderId,
                PaymentMethod = order.PaymentMethod,
                Note = order.Note,
            });
        }

        public async Task UpdateOrderStatusBasedOnPayment()
        {
            var ordersToUpdate = await _unitOfWorks.OrderRepository
                .FindByCondition(o => o.Status == OrderStatusEnum.Pending.ToString() &&
                                      o.StatusPayment == OrderStatusPaymentEnum.Pending.ToString())
                .Include(o => o.OrderDetails)
                .Include(o => o.Shipment)
                .ToListAsync();

            foreach (var order in ordersToUpdate)
            {
                if (order.PaymentMethod == PaymentMethodEnum.Cash.ToString())
                {
                    continue;
                }
                // Check thời gian
                if (order.CreatedDate != null && (DateTime.UtcNow - order.CreatedDate).TotalDays >= 1)
                {
                    // ======= Chỉ khi ĐỦ 1 NGÀY mới update =======

                    // Update Order
                    order.Status = OrderStatusEnum.Cancelled.ToString();
                    order.UpdatedDate = DateTime.UtcNow;
                    _unitOfWorks.OrderRepository.Update(order);

                    // Update OrderDetails
                    if (order.OrderDetails != null && order.OrderDetails.Any())
                    {
                        foreach (var detail in order.OrderDetails)
                        {
                            detail.Status = OrderStatusEnum.Cancelled.ToString();
                            detail.UpdatedDate = DateTime.UtcNow;
                            _unitOfWorks.OrderDetailRepository.Update(detail);
                        }
                    }

                    // Update Shipment
                    if (order.Shipment != null)
                    {
                        order.Shipment.ShippingStatus = ShippingStatusEnum.Cancelled.ToString();
                        order.Shipment.UpdatedDate = DateTime.UtcNow;
                        _unitOfWorks.ShipmentRepository.Update(order.Shipment);
                    }
                }
            }

            // Commit sau cùng
            await _unitOfWorks.OrderDetailRepository.Commit();
            await _unitOfWorks.ShipmentRepository.Commit();
            await _unitOfWorks.OrderRepository.Commit();
        }


        public async Task AutoCompleteOrderAfterDelivery()
        {
            var orders = await _unitOfWorks.OrderRepository
                .FindByCondition(o =>
                    o.Status == OrderStatusEnum.Pending.ToString() &&
                    o.StatusPayment != OrderStatusPaymentEnum.Pending.ToString() &&
                    o.Shipment != null)
                .Include(o => o.OrderDetails)
                .Include(o => o.Shipment)
                .Include(o => o.Customer)
                .ToListAsync();

            foreach (var order in orders)
            {
                DateTime deliveryDate;

                if (order.Shipment.EstimatedDeliveryDate.HasValue)
                {
                    deliveryDate = order.Shipment.EstimatedDeliveryDate.Value;
                }
                else
                {
                    deliveryDate = order.Shipment.CreatedDate;
                }

                if (DateTime.UtcNow >= deliveryDate.Date.AddDays(3))
                {
                    // ======= Cập nhật Order =======
                    order.Status = OrderStatusEnum.Completed.ToString();
                    order.UpdatedDate = DateTime.UtcNow;
                    _unitOfWorks.OrderRepository.Update(order);

                    // ======= Cập nhật OrderDetails =======
                    if (order.OrderDetails != null && order.OrderDetails.Any())
                    {
                        foreach (var detail in order.OrderDetails)
                        {
                            detail.Status = OrderStatusEnum.Completed.ToString();
                            detail.UpdatedDate = DateTime.UtcNow;
                            _unitOfWorks.OrderDetailRepository.Update(detail);
                        }
                    }

                    // ======= Cập nhật Shipment =======
                    if (order.Shipment != null)
                    {
                        order.Shipment.ShippingStatus = ShippingStatusEnum.Delivered.ToString();
                        order.Shipment.UpdatedDate = DateTime.UtcNow;
                        _unitOfWorks.ShipmentRepository.Update(order.Shipment);
                    }

                    if (order.Customer != null)
                    {
                        if (order.Customer.BonusPoint == null)
                        {
                            order.Customer.BonusPoint = 0;
                        }


                        if (order.Customer.BonusPoint < 100)
                        {
                            order.Customer.BonusPoint += 100;
                            order.Customer.UpdatedDate = DateTime.UtcNow;
                            _unitOfWorks.UserRepository.Update(order.Customer);
                        }
                    }
                }
            }


            await _unitOfWorks.OrderDetailRepository.Commit();
            await _unitOfWorks.ShipmentRepository.Commit();
            await _unitOfWorks.OrderRepository.Commit();
            await _unitOfWorks.UserRepository.Commit();
        }


        public async Task<List<RoutineAppointmentModel>> GetRoutineHistoryByCustomerIdAsync(int customerId)
        {
            // 1. Lấy các order loại Routine của customer (RoutineId != null)
            var orders = await _unitOfWorks.OrderRepository
                .FindByCondition(o => o.CustomerId == customerId
                                      && o.OrderType == "Appointment"
                                      && o.RoutineId != null)
                .Include(o => o.Routine)
                .Include(o => o.Appointments)
                .ThenInclude(a => a.Service)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            // 2. Map kết quả
            var result = orders.Select(o => new RoutineAppointmentModel
            {
                OrderId = o.OrderId,
                OrderCode = o.OrderCode,
                OrderDate = o.CreatedDate,
                Status = o.Status,
                StatusPayment = o.StatusPayment,
                TotalAmount = o.TotalAmount,
                Appointments = o.Appointments.Select(a => new AppointmentsModel
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentsTime = a.AppointmentsTime,
                    Notes = a.Notes,
                    Status = a.Status,
                    Feedback = a.Feedback,
                    UnitPrice = a.UnitPrice,
                    Quantity = a.Quantity,
                    SubTotal = a.SubTotal
                }).ToList(),
                Routine = new SkincareRoutineModel
                {
                    SkincareRoutineId = o.Routine.SkincareRoutineId,
                    Name = o.Routine.Name,
                    Description = o.Routine.Description,
                    TargetSkinTypes = o.Routine.TargetSkinTypes,
                    TotalSteps = o.Routine.TotalSteps,
                    TotalPrice = o.Routine.TotalPrice
                }
            }).ToList();

            return result;
        }

        public async Task<bool> UpdateOrderDetailStatus(int[] orderDetailIds, string status)
        {
            var orderDetails = await _unitOfWorks.OrderDetailRepository
                .FindByCondition(x => orderDetailIds.Contains(x.OrderDetailId))
                .ToListAsync();

            if (orderDetails == null || !orderDetails.Any())
            {
                throw new BadRequestException("Không tìm thấy thông tin chi tiết đơn hàng");
            }

            foreach (var orderDetail in orderDetails)
            {
                orderDetail.Status = status;
                _unitOfWorks.OrderDetailRepository.Update(orderDetail);
            }

            var result = await _unitOfWorks.OrderDetailRepository.Commit();
            return result > 0;
        }

        public async Task<OrderModel> CreateOrderBothProductAndService(
            CreateOrderWithProductsAndServicesRequest request)
        {
            await _unitOfWorks.BeginTransactionAsync();
            try
            {
                var user = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x =>
                               x.UserId == request.CustomerId && x.RoleID == 3)
                           ?? throw new BadRequestException("Không tìm thấy người dùng!");

                var voucher = request.VoucherId.HasValue
                    ? await _unitOfWorks.VoucherRepository.FirstOrDefaultAsync(x =>
                          x.VoucherId == request.VoucherId.Value) ??
                      throw new BadRequestException("Không tìm thấy mã giảm giá!")
                    : null;

                var branch = await _unitOfWorks.BranchRepository
                                 .FirstOrDefaultAsync(x => x.BranchId == request.BranchId)
                             ?? throw new BadRequestException("Không tìm thấy chi nhánh!");

                var randomOrderCode = new Random().Next(100000, 999999);
                var order = new Order
                {
                    OrderCode = randomOrderCode,
                    CustomerId = user.UserId,
                    TotalAmount = request.TotalAmount,
                    OrderType = OrderType.ProductAndService.ToString(),
                    VoucherId = request.VoucherId,
                    DiscountAmount = voucher?.DiscountAmount ?? 0,
                    Status = OrderStatusEnum.Pending.ToString(),
                    PaymentMethod = request.PaymentMethod,
                    Note = "",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                var createdOrder = await _unitOfWorks.OrderRepository.AddAsync(order);
                await _unitOfWorks.OrderRepository.Commit();

                var listAppointments = new List<Appointments>();
                var listOrderDetails = new List<OrderDetail>();

                Staff staffAuto = null;

                if (request.IsAuto)
                {
                    // Gán nhân viên bất kỳ trong chi nhánh có RoleId = 3
                    staffAuto = await _unitOfWorks.StaffRepository
                                    .FindByCondition(x => x.RoleId == 3 && x.BranchId == request.BranchId)
                                    .Include(x => x.StaffInfo)
                                    .FirstOrDefaultAsync()
                                ?? throw new BadRequestException("Không tìm thấy nhân viên phù hợp!");
                    var appointmentTime = request.AppointmentDates.FirstOrDefault();

                    if (appointmentTime < DateTime.Now)
                    {
                        throw new BadRequestException($"Thời gian đặt lịch phải lớn hơn thời gian hiện tại!");
                    }

                    for (int i = 0; i < request.ServiceIds.Length; i++)
                    {
                        var service =
                            await _unitOfWorks.ServiceRepository.FirstOrDefaultAsync(x =>
                                x.ServiceId == request.ServiceIds[i])
                            ?? throw new BadRequestException($"Không tìm thấy dịch vụ với ID {request.ServiceIds[i]}");

                        int quantity = request.ServiceQuantities[i];
                        decimal subTotal = service.Price * quantity;
                        var endTime = appointmentTime.AddMinutes(int.Parse(service.Duration) * quantity);

                        var newAppointment = new Appointments
                        {
                            CustomerId = user.UserId,
                            OrderId = createdOrder.OrderId,
                            StaffId = staffAuto.StaffId,
                            ServiceId = service.ServiceId,
                            Status = OrderStatusEnum.Pending.ToString(),
                            BranchId = request.BranchId,
                            AppointmentsTime = appointmentTime,
                            AppointmentEndTime = endTime,
                            Quantity = quantity,
                            UnitPrice = service.Price,
                            SubTotal = subTotal,
                            Feedback = "",
                            Notes = "",
                            CreatedDate = DateTime.Now
                        };
                        var appointmentEntity =
                            await _unitOfWorks.AppointmentsRepository.AddAsync(
                                _mapper.Map<Appointments>(newAppointment));
                        await _unitOfWorks.AppointmentsRepository.Commit();
                        appointmentTime = endTime;
                        
                                                // Get specialist MySQL
                        var specialistMySQL = await _staffService.GetStaffById(staffAuto.StaffId);

                        // Get admin, specialist, customer from MongoDB
                        var adminMongo = await _mongoDbService.GetCustomerByIdAsync(branch.ManagerId);
                        var specialistMongo =
                            await _mongoDbService.GetCustomerByIdAsync(specialistMySQL.StaffInfo.UserId);
                        var customerMongo = await _mongoDbService.GetCustomerByIdAsync(user.UserId);

                        // Create channel
                        var channel = await _mongoDbService.CreateChannelAsync(
                            $"Channel {appointmentEntity.AppointmentId} {service.Name}", adminMongo!.Id,
                            appointmentEntity.AppointmentId);

                        // Add member to channel
                        await _mongoDbService.AddMemberToChannelAsync(channel.Id, specialistMongo!.Id);
                        await _mongoDbService.AddMemberToChannelAsync(channel.Id, customerMongo!.Id);


                        var userMongo = await _mongoDbService.GetCustomerByIdAsync(user.UserId)
                                        ?? throw new BadRequestException(
                                            "Không tìm thấy thông tin khách hàng trong MongoDB!");

                        // create notification
                        var notification = new Notifications()
                        {
                            UserId = user.UserId,
                            Content =
                                $"Bạn có cuộc hẹn mới với {staffAuto.StaffInfo.FullName} vào lúc {newAppointment.AppointmentsTime}",
                            Type = "Appointment",
                            isRead = false,
                            ObjectId = appointmentEntity.AppointmentId,
                            CreatedDate = DateTime.UtcNow,
                        };

                        await _unitOfWorks.NotificationRepository.AddAsync(notification);
                        await _unitOfWorks.NotificationRepository.Commit();

                        if (NotificationHub.TryGetConnectionId(userMongo.Id, out var connectionId))
                        {
                            _logger.LogInformation("User connected: {userId} => {connectionId}", userMongo.Id,
                                connectionId);
                            Console.WriteLine($"User connected: {userMongo.Id} => {connectionId}");
                            await _hubContext.Clients.Client(connectionId)
                                .SendAsync("receiveNotification", notification);
                        }

                        if (NotificationHub.TryGetConnectionId(specialistMongo.Id, out var connectionSpecialListId))
                        {
                            _logger.LogInformation("User connected: {userId} => {connectionId}", specialistMongo.Id,
                                connectionId);
                            Console.WriteLine($"User connected: {specialistMongo.Id} => {connectionSpecialListId}");
                            await _hubContext.Clients.Client(connectionSpecialListId)
                                .SendAsync("receiveNotification", notification);
                        }
                    }
                }
                else
                {
                    if (request.ServiceIds.Length != request.StaffIds.Length ||
                        request.ServiceIds.Length != request.AppointmentDates.Length ||
                        request.ServiceQuantities.Length != request.ServiceIds.Length)
                        throw new BadRequestException("Số lượng dịch vụ, nhân viên, và thời gian hẹn phải tương ứng!");
                    
                    var staffAppointments =
                        new Dictionary<int, DateTime>(); // Lưu lịch làm việc của nhân viên trong request

                    for (int i = 0; i < request.ServiceIds.Length; i++)
                    {
                        var serviceId = request.ServiceIds[i];
                        var staffId = request.StaffIds[i];
                        var appointmentTime = request.AppointmentDates[i];

                        if (appointmentTime < DateTime.Now)
                        {
                            throw new BadRequestException($"Thời gian đặt lịch phải lớn hơn thời gian hiện tại!");
                        }

                        var service =
                            await _unitOfWorks.ServiceRepository.FirstOrDefaultAsync(x => x.ServiceId == serviceId);
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
                            throw new BadRequestException(
                                $"Staff hiện đang không trực ở chi nhánh {request.BranchId}!");
                        }

                        var totalDuration = int.Parse(service.Duration) + 5; // Thời gian dịch vụ + thời gian nghỉ
                        var endTime = appointmentTime.AddMinutes(totalDuration);

                        if (staffAppointments.ContainsKey(staffId))
                        {
                            var lastAppointmentEndTime = staffAppointments[staffId];
                            if (appointmentTime < lastAppointmentEndTime)
                            {
                                throw new BadRequestException(
                                    $"Staff đang bận trong khoảng thời gian: {appointmentTime}!");
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
                            .FirstOrDefaultAsync(a => a.CustomerId == user.UserId &&
                                                      a.AppointmentsTime < endTime &&
                                                      a.AppointmentEndTime > appointmentTime &&
                                                      (a.Status != OrderStatusEnum.Cancelled.ToString() &&
                                                       a.Status != OrderStatusEnum.Completed.ToString())) != null;

                        if (isCustomerBusy)
                        {
                            throw new BadRequestException(
                                $"Bạn đã có một cuộc hẹn khác trùng vào khoảng thời gian: {appointmentTime:HH:mm dd/MM/yyyy}!");
                        }


                        // Nếu IsAuto là false, kiểm tra lịch làm việc của nhân viên
                        if (!request.IsAuto)
                        {
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
                                throw new BadRequestException(
                                    $"Staff không có ca làm việc vào ngày {appointmentDate:dd/MM/yyyy}.");
                            }
                            else
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

                                // Kiểm tra xem thời gian hẹn có nằm trong khoảng ca làm không
                                var shiftStartDateTime = appointmentDate.Add(workSchedule.Shift.StartTime);
                                var shiftEndDateTime = appointmentDate.Add(workSchedule.Shift.EndTime);

                                if (appointmentTime < shiftStartDateTime || endTime > shiftEndDateTime)
                                {
                                    throw new BadRequestException(
                                        $"Thời gian đặt lịch {appointmentTime:HH:mm} không nằm trong ca làm việc ({workSchedule.Shift.ShiftName}) của nhân viên.");
                                }
                            }
                        }

                        // Tạo lịch hẹn mới
                        var newAppointment = new AppointmentsModel
                        {
                            CustomerId = user.UserId,
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
                            Feedback = "",
                            Notes = ""
                        };

                        var appointmentEntity =
                            await _unitOfWorks.AppointmentsRepository.AddAsync(
                                _mapper.Map<Appointments>(newAppointment));
                        await _unitOfWorks.AppointmentsRepository.Commit();

                        // Get specialist MySQL
                        var specialistMySQL = await _staffService.GetStaffById(staffId);

                        // Get admin, specialist, customer from MongoDB
                        var adminMongo = await _mongoDbService.GetCustomerByIdAsync(branch.ManagerId);
                        var specialistMongo =
                            await _mongoDbService.GetCustomerByIdAsync(specialistMySQL.StaffInfo.UserId);
                        var customerMongo = await _mongoDbService.GetCustomerByIdAsync(user.UserId);

                        // Create channel
                        var channel = await _mongoDbService.CreateChannelAsync(
                            $"Channel {appointmentEntity.AppointmentId} {service.Name}", adminMongo!.Id,
                            appointmentEntity.AppointmentId);

                        // Add member to channel
                        await _mongoDbService.AddMemberToChannelAsync(channel.Id, specialistMongo!.Id);
                        await _mongoDbService.AddMemberToChannelAsync(channel.Id, customerMongo!.Id);


                        var userMongo = await _mongoDbService.GetCustomerByIdAsync(user.UserId)
                                        ?? throw new BadRequestException(
                                            "Không tìm thấy thông tin khách hàng trong MongoDB!");

                        // create notification
                        var notification = new Notifications()
                        {
                            UserId = user.UserId,
                            Content =
                                $"Bạn có cuộc hẹn mới với {staff.StaffInfo.FullName} vào lúc {newAppointment.AppointmentsTime}",
                            Type = "Appointment",
                            isRead = false,
                            ObjectId = appointmentEntity.AppointmentId,
                            CreatedDate = DateTime.UtcNow,
                        };

                        await _unitOfWorks.NotificationRepository.AddAsync(notification);
                        await _unitOfWorks.NotificationRepository.Commit();

                        if (NotificationHub.TryGetConnectionId(userMongo.Id, out var connectionId))
                        {
                            _logger.LogInformation("User connected: {userId} => {connectionId}", userMongo.Id,
                                connectionId);
                            Console.WriteLine($"User connected: {userMongo.Id} => {connectionId}");
                            await _hubContext.Clients.Client(connectionId)
                                .SendAsync("receiveNotification", notification);
                        }

                        if (NotificationHub.TryGetConnectionId(specialistMongo.Id, out var connectionSpecialListId))
                        {
                            _logger.LogInformation("User connected: {userId} => {connectionId}", specialistMongo.Id,
                                connectionId);
                            Console.WriteLine($"User connected: {specialistMongo.Id} => {connectionSpecialListId}");
                            await _hubContext.Clients.Client(connectionSpecialListId)
                                .SendAsync("receiveNotification", notification);
                        }
                    }
                }


                for (int i = 0; i < request.ProductBranchIds.Length; i++)
                {
                    if (request.ProductBranchIds.Length != request.ProductQuantities.Length)
                        throw new BadRequestException("Số lượng sản phẩm và số lượng phải tương ứng!");

                    var productBranch = await _unitOfWorks.Branch_ProductRepository
                                            .FindByCondition(x =>
                                                x.Id == request.ProductBranchIds[i] && x.BranchId == branch.BranchId)
                                            .Include(x => x.Product)
                                            .FirstOrDefaultAsync()
                                        ?? throw new BadRequestException(
                                            $"Không tìm thấy sản phẩm trong chi nhánh với ID {request.ProductBranchIds[i]}");

                    var product = productBranch.Product;
                    int quantity = request.ProductQuantities[i];
                    if (productBranch.StockQuantity < quantity)
                        throw new BadRequestException($"Sản phẩm {product.ProductName} không đủ tồn kho.");
                    decimal subTotal = product.Price * quantity;

                    var newOrderDetail = new OrderDetail
                    {
                        OrderId = createdOrder.OrderId,
                        BranchId = productBranch.BranchId,
                        ProductId = product.ProductId,
                        Quantity = quantity,
                        UnitPrice = product.Price,
                        SubTotal = subTotal,
                        CreatedDate = DateTime.Now
                    };
                    listOrderDetails.Add(newOrderDetail);
                }


                /*await _unitOfWorks.AppointmentsRepository.AddRangeAsync(listAppointments);*/
                await _unitOfWorks.OrderDetailRepository.AddRangeAsync(listOrderDetails);
                await _unitOfWorks.CommitTransactionAsync();

                return _mapper.Map<OrderModel>(createdOrder);
            }
            catch (Exception)
            {
                await _unitOfWorks.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ApiResult<object>> CountOrdersByOrderTypeAsync()
        {
            var result = await _unitOfWorks.OrderRepository
                .FindByCondition(o => true)
                .GroupBy(o => o.OrderType)
                .Select(g => new
                {
                    OrderType = g.Key,
                    Amount = g.Count()
                })
                .ToListAsync();
            return ApiResult<object>.Succeed(new
            {
                OrderTypeCounts = result
            });
        }

        //public async Task AutoCancelPendingAppointmentOrdersAsync()
        //{
        //    var utc = DateTime.UtcNow;

        //    var orders = await _unitOfWorks.OrderRepository
        //        .FindByCondition(o =>
        //            o.OrderType == OrderType.Appointment.ToString() &&
        //            o.Status == OrderStatusEnum.Pending.ToString() &&
        //            o.StatusPayment == OrderStatusPaymentEnum.Pending.ToString() &&
        //            o.CreatedDate <= utc.AddMinutes(-15)) 
        //        .Include(o => o.Appointments)
        //        .ToListAsync();

        //    foreach (var order in orders)
        //    {
        //        // ======= Cập nhật Order =======
        //        order.Status = OrderStatusEnum.Cancelled.ToString();
        //        order.UpdatedDate = DateTime.UtcNow;
        //        _unitOfWorks.OrderRepository.Update(order);

        //        // ======= Cập nhật Appointments =======
        //        if (order.Appointments != null && order.Appointments.Any())
        //        {
        //            foreach (var appointment in order.Appointments)
        //            {
        //                appointment.Status = OrderStatusEnum.Cancelled.ToString();
        //                appointment.UpdatedDate = DateTime.UtcNow;
        //                _unitOfWorks.AppointmentsRepository.Update(appointment);
        //            }
        //        }
        //    }

        //    // ======= Commit tất cả =======
        //    await _unitOfWorks.OrderRepository.Commit();
        //    await _unitOfWorks.AppointmentsRepository.Commit();
        //}

        public async Task AutoCancelPendingAppointmentOrdersAsync()
        {
            var utcNow = DateTime.UtcNow;
            var thresholdTime = utcNow.AddMinutes(-15);

            var orders = await _unitOfWorks.OrderRepository
                .FindByCondition(o =>
                    (o.OrderType.ToUpper() == OrderType.Appointment.ToString().ToUpper() ||
                     o.OrderType.ToUpper() == OrderType.Routine.ToString().ToUpper() ||
                     o.OrderType.ToUpper() == OrderType.ProductAndService.ToString().ToUpper()) &&
                    o.Status.ToUpper() == OrderStatusEnum.Pending.ToString().ToUpper() &&
                    o.StatusPayment.ToUpper() == OrderStatusPaymentEnum.Pending.ToString().ToUpper() &&
                    o.CreatedDate <= thresholdTime)
                .Include(o => o.Appointments)
                .ThenInclude(a => a.Customer)
                .AsNoTracking()
                .ToListAsync();

            foreach (var order in orders)
            {
                if (order.PaymentMethod?.ToUpper() == PaymentMethodEnum.Cash.ToString().ToUpper())
                    continue;

                var orderEntity = await _unitOfWorks.OrderRepository.GetByIdAsync(order.OrderId);
                if (orderEntity != null)
                {
                    orderEntity.Status = OrderStatusEnum.Cancelled.ToString();
                    orderEntity.UpdatedDate = utcNow;
                    _unitOfWorks.OrderRepository.Update(orderEntity);
                }

                // Lưu flag gửi mail + message theo customerId
                var notifiedCustomers = new Dictionary<int, string>();

                foreach (var appointment in order.Appointments)
                {
                    var appointmentEntity =
                        await _unitOfWorks.AppointmentsRepository.GetByIdAsync(appointment.AppointmentId);
                    if (appointmentEntity != null)
                    {
                        appointmentEntity.Status = OrderStatusEnum.Cancelled.ToString();
                        appointmentEntity.UpdatedDate = utcNow;
                        _unitOfWorks.AppointmentsRepository.Update(appointmentEntity);
                    }

                    var customer = appointment.Customer;
                    if (customer != null && !string.IsNullOrWhiteSpace(customer.Email))
                    {
                        var content = string.IsNullOrWhiteSpace(appointment.Step?.ToString())
                            ? "Lịch hẹn của bạn đã bị hủy do chưa thanh toán đúng hạn. Vui lòng tạo lại lịch mới nếu cần."
                            : "Lịch hẹn của bạn đã bị hủy. Vui lòng liên hệ với quản lý để được hỗ trợ xử lý bước liệu trình.";

                        if (!notifiedCustomers.ContainsKey(customer.UserId))
                            notifiedCustomers[customer.UserId] = content;
                    }
                }

                // Gửi duy nhất 1 email cho mỗi customer
                foreach (var (customerId, message) in notifiedCustomers)
                {
                    var customer = order.Appointments.FirstOrDefault(a => a.Customer?.UserId == customerId)?.Customer;
                    if (customer == null) continue;

                    var mailData = new MailData
                    {
                        EmailToId = customer.Email,
                        EmailToName = customer.FullName,
                        EmailSubject = "Thông báo hủy lịch hẹn",
                        EmailBody = $@"
<p>Chào {customer.FullName},</p>
<p>{message}</p>
<p>Trân trọng,</p>
<p>Đội ngũ Solace Spa</p>"
                    };

                    _ = Task.Run(async () =>
                    {
                        var result = await _mailService.SendEmailAsync(mailData, false);
                        if (!result)
                            Console.WriteLine($"❌ Gửi email thất bại cho: {customer.Email}");
                    });
                }
            }


            await _unitOfWorks.OrderRepository.Commit();
            await _unitOfWorks.AppointmentsRepository.Commit();
        }


        public async Task<bool> UpdateStatusPayment(int orderId, string statusPaymentEnum)
        {
            if (!Enum.TryParse<OrderStatusPaymentEnum>(statusPaymentEnum, true, out var parsedStatus))
            {
                throw new BadRequestException("Trạng thái thanh toán không hợp lệ!");
            }

            var order = await _unitOfWorks.OrderRepository.FirstOrDefaultAsync(x => x.OrderId == orderId)
                        ?? throw new BadRequestException("Không tìm thấy đơn hàng!");

            order.StatusPayment = parsedStatus.ToString();
            _unitOfWorks.OrderRepository.Update(order);
            var result = await _unitOfWorks.SaveChangesAsync();

            // Gửi notification cho khách hàng
            var customer = await _unitOfWorks.UserRepository.GetByIdAsync(order.CustomerId);
            if (customer != null)
            {
                var notification = new Notifications()
                {
                    UserId = customer.UserId,
                    Content = $"Trạng thái thanh toán cho đơn hàng #{order.OrderId} đã được cập nhật thành: {parsedStatus}",
                    Type = "OrderPayment",
                    isRead = false,
                    ObjectId = order.OrderId,
                    CreatedDate = DateTime.UtcNow,
                };

                await _unitOfWorks.NotificationRepository.AddAsync(notification);
                await _unitOfWorks.NotificationRepository.Commit();

                // Gửi real-time nếu đang kết nối
                var userMongo = await _mongoDbService.GetCustomerByIdAsync(customer.UserId);
                if (NotificationHub.TryGetConnectionId(userMongo.Id, out var connectionId))
                {
                    _logger.LogInformation("User connected: {userId} => {connectionId}", userMongo.Id, connectionId);
                    await _hubContext.Clients.Client(connectionId).SendAsync("receiveNotification", notification);
                }
            }

            return result > 0;
        }

    }
}