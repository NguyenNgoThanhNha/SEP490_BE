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
using Server.Business.Ultils;
using Server.Data;

namespace Server.Business.Services
{
    public class OrderService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly PayOSSetting _payOsSetting;

        public OrderService(UnitOfWorks unitOfWorks, IMapper mapper, IOptions<PayOSSetting> payOsSetting)
        {
            this._unitOfWorks = unitOfWorks;
            _mapper = mapper;
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
     .FindByCondition(x => x.OrderCode == model.OrderCode && x.Status == OrderStatusEnum.Pending.ToString())
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
        if(order == null)
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
        if(order == null)
        {
            throw new BadRequestException("Order not found!");
        }
        
        // Lấy danh sách tất cả Appointments theo OrderId
        var appointments = await _unitOfWorks.AppointmentsRepository
            .FindByCondition(x => x.OrderId == req.orderId)
            .Include(x => x.Service) // Include Service để lấy thông tin dịch vụ
            .Include(x => x.Branch)  // Include Branch để lấy thông tin chi nhánh (nếu cần)
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
        if (!decimal.TryParse(req.percent, out decimal depositPercent) || depositPercent <= 0 || depositPercent > 100)
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
        order.Note = $"Đặt cọc {depositPercent}% với số tiền: {depositAmount:C}";
        order.Status = "Deposit Pending";
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
            description: $"Deposit - Order {order.OrderCode}",
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

    }
}
