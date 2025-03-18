﻿using AutoMapper;
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

namespace Server.Business.Services
{
    public class OrderService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly ServiceService _serviceService;
        private readonly PayOSSetting _payOsSetting;

        public OrderService(UnitOfWorks unitOfWorks, IMapper mapper, IOptions<PayOSSetting> payOsSetting, ServiceService serviceService)
        {
            this._unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _serviceService = serviceService;
            _payOsSetting = payOsSetting.Value;
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
                    return ApiResult<object>.Error(null, "Order code already exists.");
                }

                // Kiểm tra tồn tại khách hàng
                var isCustomerExists = await _unitOfWorks.UserRepository
                    .FindByCondition(x => x.UserId == model.CustomerId &&
                                          x.RoleID == (int)RoleConstant.RoleType.Customer &&
                                          x.Status == "Active")
                    .AnyAsync();

                if (!isCustomerExists)
                {
                    return ApiResult<object>.Error(null, "Customer not found.");
                }

                // Kiểm tra tồn tại voucher (nếu có)
                int? voucherId = null;
                if (model.VoucherId.HasValue && model.VoucherId.Value > 0)
                {
                    var isVoucherExists = await _unitOfWorks.VoucherRepository
                        .FindByCondition(x => x.VoucherId == model.VoucherId && x.Status == "Active")
                        .AnyAsync();

                    if (!isVoucherExists)
                    {
                        return ApiResult<object>.Error(null, "Voucher not found.");
                    }

                    voucherId = model.VoucherId; // Chỉ gán nếu voucher hợp lệ
                }

                // Tạo đơn hàng mới
                var order = new Order
                {
                    OrderCode = model.OrderCode,
                    CustomerId = model.CustomerId,
                    VoucherId = voucherId ?? 0,
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
                    return ApiResult<object>.Error(null, "Failed to fetch created order details.");
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
                return ApiResult<object>.Error(null, $"An error occurred: {ex.Message}");
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
            var totalAmount = orderDetails.Sum(od => od.Quantity * od.UnitPrice);

            // Xác minh tổng tiền với giá trị từ request (nếu cần)
            if (Convert.ToDecimal(req.totalAmount) != totalAmount)
            {
                throw new BadRequestException("Total amount mismatch!");
            }

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
            var totalAmount = appointments.Sum(ap => ap.Quantity * ap.UnitPrice);

            // Xác minh tổng tiền với giá trị từ request (nếu cần)
            if (Convert.ToDecimal(req.totalAmount) != totalAmount)
            {
                throw new BadRequestException("Total amount mismatch!");
            }

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

        public async Task<HistoryBookingResponse> BookingHistory(int userId, string status, int page = 1,
            int pageSize = 5)
        {
            var listOrders = await _unitOfWorks.OrderRepository.FindByCondition(x => x.CustomerId == userId)
                .Include(x => x.Customer)
                .Include(x => x.Voucher)
                .Include(x => x.Appointments)
                .ThenInclude(x => x.Service)
                .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Product)
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

        public async Task<DetailOrderResponse> GetDetailOrder(int orderId, int userId)
        {
            var order = await _unitOfWorks.OrderRepository.FindByCondition(x => x.OrderId == orderId && x.CustomerId == userId)
                .FirstOrDefaultAsync();
            var listService = new List<Data.Entities.Service>();
            var listSerivceModels = new List<ServiceModel>();
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
                
                // get list service images
                foreach (var appointment in orderAppointments)
                {
                    listService.Add(appointment.Service);
                }

                listSerivceModels = await _serviceService.GetListImagesOfServices(listService);
            }else if (order.OrderType == "Product")
            {
                var orderDetails = await _unitOfWorks.OrderDetailRepository
                    .FindByCondition(x => x.OrderId == orderId)
                    .Include(x => x.Product)
                    .ToListAsync();
                order.OrderDetails = orderDetails;
            }
            
            if (order == null)
            {
                return null;
            }

            var orderModel = _mapper.Map<OrderModel>(order);
            if (orderModel.Appointments.Any())
            {
                foreach (var appointment in orderModel.Appointments)
                {
                    foreach (var serviceModel in listSerivceModels)
                    {
                        if (appointment.ServiceId == serviceModel.ServiceId)
                        {
                            appointment.Service.images = serviceModel.images;
                        }
                    }
                }
            }
            return new DetailOrderResponse()
            {
                message = "Get detail order success",
                data = orderModel
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
                throw new BadRequestException($"Status must be one of: {string.Join(", ", Enum.GetNames(typeof(OrderStatusEnum)))}.");
            }
            
            if (!string.IsNullOrEmpty(request.Status) && !Enum.IsDefined(typeof(OrderStatusPaymentEnum), request.Status))
            {
                throw new BadRequestException($"Status Order Payment must be one of: {string.Join(", ", Enum.GetNames(typeof(OrderStatusPaymentEnum)))}.");
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

        public async Task<ApiResult<string>> CancelOrderAsync(int orderId)
        {
            var order = await _unitOfWorks.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return ApiResult<string>.Error(null, "Order not found.");
            }

            // Validate thời gian – chỉ huỷ trong 24h từ lúc tạo đơn
            if (!CanModifyOrder(order.CreatedDate, 24))
            {
                return ApiResult<string>.Error(null, "You can only cancel the order within 24 hours of creation.");
            }

            // Chỉ huỷ khi đơn chưa thanh toán hoặc chưa xử lý
            if (order.Status != OrderStatusEnum.Pending.ToString())
            {
                return ApiResult<string>.Error(null, "Only pending orders can be canceled.");
            }

            order.Status = OrderStatusEnum.Cancelled.ToString();
            order.UpdatedDate = DateTime.UtcNow;

            _unitOfWorks.OrderRepository.Update(order);
            await _unitOfWorks.OrderRepository.Commit();

            return ApiResult<string>.Succeed("Order canceled successfully.");
        }

        private bool CanModifyOrder(DateTime createdDate, int allowedHours = 24)
        {
            var timeElapsed = DateTime.UtcNow - createdDate;
            return timeElapsed.TotalHours <= allowedHours;
        }

    }
}
