using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Dtos;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services
{
    public class OrderDetailService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public OrderDetailService(UnitOfWorks unitOfWorks, IMapper mapper, AppDbContext context)
        {
            this._unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _context = context;
        }

        public async Task<ApiResult<OrderDetail>> CreateOrderDetailAsync(CUOrderDetailDto model)
        {
            if (!await _context.Orders.AnyAsync(x => x.OrderId == model.OrderId))
            {
                return ApiResult<OrderDetail>.Error(null, "Order not found");
            }
            if (!await _context.Products.AnyAsync(x => x.ProductId == model.ProductId && x.Status == "Active"))
            {
                return ApiResult<OrderDetail>.Error(null, "Product not found");
            }
            if (!await _context.Services.AnyAsync(x => x.ServiceId == model.ServiceId && x.Status == "Active"))
            {
                return ApiResult<OrderDetail>.Error(null, "Service not found");
            }

            var order = _mapper.Map<OrderDetail>(model);
            try
            {
                _context.OrderDetails.Add(order);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return ApiResult<OrderDetail>.Error(null, ex.Message.ToString());
            }
            return ApiResult<OrderDetail>.Succeed(order);
        }
    }
}
