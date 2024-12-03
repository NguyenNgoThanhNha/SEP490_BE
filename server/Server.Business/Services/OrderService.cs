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
        private readonly AppDbContext _context;

        public OrderService(UnitOfWorks unitOfWorks, IMapper mapper, AppDbContext context)
        {
            this._unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _context = context;
        }


        public async Task<Pagination<Order>> GetListAsync(Expression<Func<Order, bool>> filter = null,
                                    Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = null,
                                    string includeProperties = "",
                                    int? pageIndex = null, // Optional parameter for pagination (page number)
                                    int? pageSize = null)
        {
            IQueryable<Order> query = _context.Orders;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var totalItemsCount = await query.CountAsync();

            if (pageIndex.HasValue && pageIndex.Value == -1)
            {
                pageSize = totalItemsCount; // Set pageSize to total count
                pageIndex = 0; // Reset pageIndex to 0
            }
            else if (pageIndex.HasValue && pageSize.HasValue)
            {
                int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value : 0;
                int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10; // Assuming a default pageSize of 10 if an invalid value is passed

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


        public async Task<ApiResponse> CreateOrderAsync(CUOrderDto model)
        {
            if (await _context.Orders.AnyAsync(x => x.OrderCode == model.OrderCode && x.Status == "Active"))
            {
                return ApiResponse.Error("Order code is existed");
            }
            if (!await _context.Users.AnyAsync(x => x.UserId == model.CustomerId && x.RoleID == (int)RoleConstant.RoleType.Customer && x.Status == "Active"))
            {
                return ApiResponse.Error("Customer not found");
            }
            if (model.VoucherId != 0 && !await _context.Vouchers.AnyAsync(x => x.VoucherId == model.VoucherId && x.Status == "Active"))
            {
                return ApiResponse.Error("Voucher not found");
            }

            var order = _mapper.Map<Order>(model);
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message.ToString());
            }
            return ApiResponse.Succeed(order);
        }


    }
}
