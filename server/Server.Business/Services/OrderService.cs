using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Constants;
using Server.Business.Dtos;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System.Linq.Expressions;

namespace Server.Business.Services
{
    public class OrderService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public OrderService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            this._unitOfWorks = unitOfWorks;
            _mapper = mapper;
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
     .FindByCondition(x => x.OrderCode == model.OrderCode && x.Status == "Active")
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
                    Status = "Pending",
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
    }
}
