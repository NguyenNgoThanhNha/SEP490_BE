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
using Server.Data.MongoDb.Models;
using CloudinaryDotNet.Core;
using Microsoft.EntityFrameworkCore.Query;

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

        public OrderService(UnitOfWorks unitOfWorks, IMapper mapper, IOptions<PayOSSetting> payOsSetting,
            ServiceService serviceService, AuthService authService, ProductService productService)
        {
            this._unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _serviceService = serviceService;
            _payOsSetting = payOsSetting.Value;
            _authService = authService;
            _productService = productService;
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
                    OrderType = "Product",
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


        public async Task<HistoryBookingResponse> BookingHistory(int userId, string status, string orderType,
            int page = 1,
            int pageSize = 5)
        {
            var listOrders = await _unitOfWorks.OrderRepository
                .FindByCondition(x => x.CustomerId == userId)
                .Include(x => x.Customer)
                .Include(x => x.Routine)
                .Include(x => x.Voucher)
                .Include(x => x.Shipment) // ✅ Thêm Include Shipment
                .Include(x => x.Appointments)
                .ThenInclude(x => x.Service)
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.ProductImages) // ✅ Hình ảnh sản phẩm
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.Branch_Products)
                .ThenInclude(bp => bp.Branch) // ✅ Chi nhánh của sản phẩm
                .Where(x => x.Status == status && x.OrderType == orderType)
                .OrderByDescending(x => x.CreatedDate)
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
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.ProductImages)
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.Branch_Products)
                .ThenInclude(bp => bp.Branch)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            if (listOrders == null || !listOrders.Any())
            {
                return new HistoryBookingResponse()
                {
                    data = null,
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

            var orderModels = _mapper.Map<List<OrderModel>>(pagedOrders);

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


        public async Task<DetailOrderResponse> GetDetailOrder(int orderId, int userId)
        {
            var order = await _unitOfWorks.OrderRepository
                .FindByCondition(x => x.OrderId == orderId && x.CustomerId == userId)
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
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return null; // Controller xử lý lỗi
            }

            var listService = new List<Data.Entities.Service>();
            var listProduct = new List<Product>();
            var listServiceModels = new List<ServiceModel>();
            var listProductModels = new List<ProductModel>();

            if (order.OrderType == "Appointment")
            {
                var orderAppointments = await _unitOfWorks.AppointmentsRepository
                    .FindByCondition(x => x.OrderId == orderId)
                    .Include(x => x.Branch)
                    .Include(x => x.Service)
                    .Include(x => x.Staff)
                    .ThenInclude(x => x.StaffInfo)
                    .ToListAsync();

                order.Appointments = orderAppointments;
                listService = orderAppointments.Select(a => a.Service).ToList();
                listServiceModels = await _serviceService.GetListImagesOfServices(listService);
            }
            else if (order.OrderType == "Product")
            {
                var orderDetails = order.OrderDetails.ToList();
                listProduct = orderDetails.Select(od => od.Product).ToList();
                listProductModels = await _productService.GetListImagesOfProduct(listProduct);
            }

            var orderModel = _mapper.Map<OrderModel>(order);

            if (orderModel.Appointments.Any())
            {
                foreach (var appointment in orderModel.Appointments)
                {
                    var matchedService = listServiceModels.FirstOrDefault(s => s.ServiceId == appointment.ServiceId);
                    if (matchedService != null)
                    {
                        appointment.Service.images = matchedService.images;
                    }
                }
            }
            else if (orderModel.OrderDetails.Any())
            {
                foreach (var orderDetail in orderModel.OrderDetails)
                {
                    var matchedProduct =
                        listProductModels.FirstOrDefault(p => p.ProductId == orderDetail.Product.ProductId);
                    if (matchedProduct != null)
                    {
                        orderDetail.Product.images = matchedProduct.images;
                        orderDetail.Product.Branches = matchedProduct.Branches;
                    }
                }
            }

            return new DetailOrderResponse
            {
                message = "Get detail order success",
                data = orderModel // ✅ Là 1 object, không phải list
            };
        }


        public async Task<bool> CreateMoreOrderAppointment(int orderId, AppointmentUpdateRequest request)
        {
            var order = await _unitOfWorks.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return false;
            }

            // Kiểm tra nếu Status không hợp lệ
            if (!string.IsNullOrEmpty(request.Status) && !Enum.IsDefined(typeof(OrderStatusEnum), request.Status))
            {
                throw new BadRequestException(
                    $"Status must be one of: {string.Join(", ", Enum.GetNames(typeof(OrderStatusEnum)))}.");
            }

            if (!string.IsNullOrEmpty(request.Status) &&
                !Enum.IsDefined(typeof(OrderStatusPaymentEnum), request.Status))
            {
                throw new BadRequestException(
                    $"Status Order Payment must be one of: {string.Join(", ", Enum.GetNames(typeof(OrderStatusPaymentEnum)))}.");
            }

            var appointment = new Appointments()
            {
                CustomerId = request.CustomerId,
                OrderId = order.OrderId,
                BranchId = request.BranchId,
                ServiceId = request.ServiceId,
                StaffId = request.StaffId,
                AppointmentsTime = request.AppointmentsTime,
                Status = request.Status ?? OrderStatusEnum.Pending.ToString(),
                StatusPayment = request.StatusPayment ?? OrderStatusPaymentEnum.Pending.ToString(),
                Notes = request.Notes ?? "",
                Feedback = request?.Feedback ?? "",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            await _unitOfWorks.AppointmentsRepository.AddAsync(appointment);
            return await _unitOfWorks.AppointmentsRepository.Commit() > 0;
        }

        public async Task<bool> CreateMoreOrderProduct(int orderId, OrderDetailRequest request)
        {
            var order = await _unitOfWorks.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return false;
            }

            var product = await _unitOfWorks.ProductRepository.GetByIdAsync(request.ProductId);

            var productPrice = product?.Price ?? 0;

            var orderDetail = new OrderDetail
            {
                OrderId = orderId,
                ProductId = request.ProductId,
                PromotionId = request.PromotionId > 0 ? request.PromotionId : null,
                Quantity = request.Quantity,
                UnitPrice = productPrice,
                SubTotal = request.Quantity * productPrice,
                Status = request.Status ?? OrderStatusEnum.Pending.ToString(),
                StatusPayment = request.StatusPayment ?? OrderStatusPaymentEnum.Pending.ToString(),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            await _unitOfWorks.OrderDetailRepository.AddAsync(orderDetail);
            return await _unitOfWorks.OrderDetailRepository.Commit() > 0;
        }

        public async Task<bool> UpdateOrderStatus(int orderId, string orderStatus)
        {
            var order = await _unitOfWorks.OrderRepository.GetByIdAsync(orderId);
            if (order == null) throw new BadRequestException("Order not found!");

            if (order.Status == OrderStatusEnum.Completed.ToString())
            {
                throw new BadRequestException("Đơn hàng đã hoàn thành không thể thay đổi trạng thái!");
            }

            var customer = await _unitOfWorks.UserRepository.GetByIdAsync(order.CustomerId);
            customer.BonusPoint += 200;
            _unitOfWorks.UserRepository.Update(customer);
            await _unitOfWorks.UserRepository.Commit();

            order.Status = orderStatus;
            order.UpdatedDate = DateTime.Now;

            _unitOfWorks.OrderRepository.Update(order);
            return await _unitOfWorks.OrderRepository.Commit() > 0;
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

        public async Task<Pagination<OrderModel>> GetListOrderFilterAsync(GetAllOrderRequest request)
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
                .ThenInclude(x => x.ServiceCategory);

            if (request.BranchId.HasValue)
            {
                query = query.Where(order =>
                    (order.OrderType == "Appointment" &&
                     order.Appointments.Any(a => a.BranchId == request.BranchId.Value)) ||
                    (order.OrderType == "Product" &&
                     order.OrderDetails.Any(od =>
                         od.Product.Branch_Products.Any(bp => bp.BranchId == request.BranchId.Value)))
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
                if (order.OrderType == "Appointment")
                {
                    var services = order.Appointments.Select(a => a.Service).ToList();
                    var serviceModels = await _serviceService.GetListImagesOfServices(_mapper.Map<List<Data.Entities.Service>>(services));
                    foreach (var appointment in order.Appointments)
                    {
                        var matchedService = serviceModels.FirstOrDefault(s => s.ServiceId == appointment.ServiceId);
                        if (matchedService != null)
                            appointment.Service.images = matchedService.images;
                    }
                }
                else if (order.OrderType == "Product")
                {
                    var products = order.OrderDetails.Select(od => od.Product).ToList();
                    var productModels = await _productService.GetListImagesOfProduct(_mapper.Map<List<Product>>(products));
                    foreach (var detail in order.OrderDetails)
                    {
                        var matchedProduct = productModels.FirstOrDefault(p => p.ProductId == detail.Product.ProductId);
                        if (matchedProduct != null)
                        {
                            detail.Product.images = matchedProduct.images;
                            detail.Product.Branches = matchedProduct.Branches;
                        }
                    }
                }
                else if(order.OrderType == "Routine")
                {
                    var services = order.Appointments.Select(a => a.Service).ToList();
                    var serviceModels = await _serviceService.GetListImagesOfServices(_mapper.Map<List<Data.Entities.Service>>(services));
                    foreach (var appointment in order.Appointments)
                    {
                        var matchedService = serviceModels.FirstOrDefault(s => s.ServiceId == appointment.ServiceId);
                        if (matchedService != null)
                            appointment.Service.images = matchedService.images;
                    }
                    
                    var products = order.OrderDetails.Select(od => od.Product).ToList();
                    var productModels = await _productService.GetListImagesOfProduct(_mapper.Map<List<Product>>(products));
                    foreach (var detail in order.OrderDetails)
                    {
                        var matchedProduct = productModels.FirstOrDefault(p => p.ProductId == detail.Product.ProductId);
                        if (matchedProduct != null)
                        {
                            detail.Product.images = matchedProduct.images;
                            detail.Product.Branches = matchedProduct.Branches;
                        }
                    }
                }
            }

            return new Pagination<OrderModel>
            {
                TotalItemsCount = totalItemsCount,
                PageSize = pageSize,
                PageIndex = pageIndex,
                Data = result
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


        public async Task<ApiResult<object>> CreateOrderWithDetailsAsync(CreateOrderWithDetailsRequest request)
        {
            if (request == null
                || request.Products == null
                || !request.Products.Any()
                || string.IsNullOrWhiteSpace(request.PaymentMethod))
            {
                return ApiResult<object>.Error(null, "Vui lòng nhập đầy đủ thông tin đơn hàng.");
            }

            using var transaction = await _unitOfWorks.BeginTransactionAsync();

            try
            {
                // 1. Lấy user
                var user = await _unitOfWorks.UserRepository
                    .FindByCondition(x => x.UserId == request.UserId && x.Status == "Active")
                    .FirstOrDefaultAsync();

                if (user == null)
                    return ApiResult<object>.Error(null, "User không tồn tại hoặc đang bị vô hiệu hóa.");

                // 2. Kiểm tra voucher
                int? voucherId = null;
                if (request.VoucherId.HasValue)
                {
                    var voucher = await _unitOfWorks.VoucherRepository
                        .FindByCondition(x => x.VoucherId == request.VoucherId && x.Status == "Active")
                        .FirstOrDefaultAsync();

                    if (voucher == null)
                    {
                        return ApiResult<object>.Succeed(new ApiResponse
                        {
                            message = "Mã giảm giá không hợp lệ.",
                            data = null
                        });
                    }

                    voucherId = voucher.VoucherId;
                }

                // 3. Tạo OrderCode ngẫu nhiên
                int orderCode;
                var random = new Random();
                do
                {
                    orderCode = random.Next(1000, 10000);
                } while (await _unitOfWorks.OrderRepository
                             .FindByCondition(x => x.OrderCode == orderCode)
                             .AnyAsync());

                // 4. Tạo Order (chưa có TotalAmount)
                var order = new Order
                {
                    OrderCode = orderCode,
                    CustomerId = request.UserId,
                    VoucherId = voucherId,
                    TotalAmount = 0, // Tạm thời
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

                // 5. Tạo OrderDetail và cập nhật tồn kho
                foreach (var item in request.Products)
                {
                    if (item.Quantity <= 0)
                        throw new BadRequestException(
                            $"Số lượng của sản phẩm [ProductBranchId = {item.ProductBranchId}] phải lớn hơn 0.");

                    var branchProduct = await _unitOfWorks.Branch_ProductRepository.GetByIdAsync(item.ProductBranchId);
                    if (branchProduct == null)
                        throw new BadRequestException($"Không tìm thấy BranchProduct với ID = {item.ProductBranchId}.");

                    if (branchProduct.StockQuantity < item.Quantity)
                        throw new BadRequestException($"Sản phẩm ID {branchProduct.ProductId} không đủ tồn kho.");

                    var product = await _unitOfWorks.ProductRepository.GetByIdAsync(branchProduct.ProductId);
                    if (product == null)
                        throw new BadRequestException($"Không tìm thấy Product với ID = {branchProduct.ProductId}.");

                    decimal subTotal = product.Price * item.Quantity;
                    calculatedTotal += subTotal;

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        SubTotal = subTotal,
                        Status = OrderStatusEnum.Pending.ToString(),
                        StatusPayment = OrderStatusPaymentEnum.Pending.ToString(),
                        PaymentMethod = request.PaymentMethod,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    await _unitOfWorks.OrderDetailRepository.AddAsync(orderDetail);

                    // Cập nhật tồn kho
                    branchProduct.StockQuantity -= item.Quantity;
                    branchProduct.UpdatedDate = DateTime.Now;
                    _unitOfWorks.Branch_ProductRepository.Update(branchProduct);
                }

                // 6. Chuẩn hóa thông tin giao hàng
                var recipientName = string.IsNullOrWhiteSpace(request.RecipientName)
                    ? user.FullName
                    : request.RecipientName;
                var recipientAddress = string.IsNullOrWhiteSpace(request.RecipientAddress)
                    ? user.Address
                    : request.RecipientAddress;
                var recipientPhone = string.IsNullOrWhiteSpace(request.RecipientPhone)
                    ? user.PhoneNumber
                    : request.RecipientPhone;

                // 7. Tạo Shipment
                var shipment = new Shipment
                {
                    OrderId = order.OrderId,
                    EstimatedDeliveryDate = request.EstimatedDeliveryDate,
                    ShippingCost = request.ShippingCost,
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

                // 8. Cộng ship cost vào tổng
                calculatedTotal += shipment.ShippingCost;

                // 9. Cập nhật TotalAmount vào bảng Order
                order.TotalAmount = calculatedTotal;
                order.UpdatedDate = DateTime.Now;
                _unitOfWorks.OrderRepository.Update(order);
                await _unitOfWorks.OrderRepository.Commit();

                // 10. Commit tất cả
                await _unitOfWorks.OrderDetailRepository.Commit();
                await _unitOfWorks.ShipmentRepository.Commit();
                await _unitOfWorks.Branch_ProductRepository.Commit();
                await transaction.CommitAsync();

                return ApiResult<object>.Succeed(new
                {
                    message = "Tạo đơn hàng thành công.",
                    data = order.OrderId
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResult<object>.Error(null, $"Lỗi tạo đơn hàng: {ex.Message}");
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


        public async Task<ApiResult<object>> UpdatePaymentMethodOrNoteAsync(UpdatePaymentMethodOrNoteRequest request,
            string token)
        {
            // Lấy thông tin người dùng từ token
            var currentUser = await _authService.GetUserInToken(token);
            if (currentUser == null)
            {
                return ApiResult<object>.Error(null, "Token không hợp lệ hoặc người dùng không tìm thấy.");
            }

            // Lấy thông tin đơn hàng
            var order = await _unitOfWorks.OrderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return ApiResult<object>.Error(null, "Không tìm thấy đơn hàng.");
            }

            // Kiểm tra quyền của người dùng
            if (order.CustomerId != currentUser.UserId)
            {
                return ApiResult<object>.Error(null, "Bạn không có quyền cập nhật đơn hàng này.");
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
            // Lấy các đơn hàng có trạng thái "Pending" và chưa thanh toán
            var ordersToUpdate = await _unitOfWorks.OrderRepository
                .FindByCondition(o => o.Status == OrderStatusEnum.Pending.ToString() &&
                                      o.StatusPayment == OrderStatusPaymentEnum.Pending.ToString())
                .Include(o => o.OrderDetails)
                .Include(o => o.Shipment)
                .ToListAsync();

            foreach (var order in ordersToUpdate)
            {
                // Check thời gian
                if (order.CreatedDate != null && (DateTime.UtcNow - order.CreatedDate).TotalDays >= 1)
                {
                    // ======= Cập nhật Order =======
                    order.Status = OrderStatusEnum.Cancelled.ToString();
                    order.UpdatedDate = DateTime.UtcNow;
                    _unitOfWorks.OrderRepository.Update(order);

                    // ======= Cập nhật OrderDetails =======
                    if (order.OrderDetails != null && order.OrderDetails.Any())
                    {
                        foreach (var detail in order.OrderDetails)
                        {
                            detail.Status = OrderStatusEnum.Cancelled.ToString();
                            detail.UpdatedDate = DateTime.UtcNow;
                            _unitOfWorks.OrderDetailRepository.Update(detail);
                        }
                    }

                    // ======= Cập nhật Shipment =======
                    if (order.Shipment != null)
                    {
                        order.Shipment.ShippingStatus = ShippingStatusEnum.Cancelled.ToString();
                        order.Shipment.UpdatedDate = DateTime.UtcNow;
                        _unitOfWorks.ShipmentRepository.Update(order.Shipment);
                    }
                }
            }

            // ======= Commit =======
            await _unitOfWorks.OrderDetailRepository.Commit();
            await _unitOfWorks.ShipmentRepository.Commit();
            await _unitOfWorks.OrderRepository.Commit();
        }


        public async Task AutoCompleteOrderAfterDelivery()
        {
            // Lấy các đơn hàng đủ điều kiện auto-complete
            var orders = await _unitOfWorks.OrderRepository
                .FindByCondition(o => o.Status == OrderStatusEnum.Pending.ToString()
                                      && o.StatusPayment == OrderStatusPaymentEnum.Paid.ToString()
                                      && o.Shipment != null
                                      && o.Shipment.EstimatedDeliveryDate != null)
                .Include(o => o.OrderDetails)
                .Include(o => o.Shipment)
                .ToListAsync();

            foreach (var order in orders)
            {
                // Kiểm tra nếu đã quá 3 ngày sau ngày dự kiến giao hàng
                if (order.Shipment.EstimatedDeliveryDate.HasValue &&
                    DateTime.UtcNow.Date >= order.Shipment.EstimatedDeliveryDate.Value.Date.AddDays(3))
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
                }
            }

            // ======= Commit =======
            await _unitOfWorks.OrderDetailRepository.Commit();
            await _unitOfWorks.ShipmentRepository.Commit();
            await _unitOfWorks.OrderRepository.Commit();
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
    }
}