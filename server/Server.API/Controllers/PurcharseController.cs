using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Services;
using Server.Data.Entities;
using System.Linq.Expressions;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurcharseController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly AppDbContext _context;
        public PurcharseController(OrderService orderService, AppDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(int? orderCode,
            string? customerName,
            string? voucherCode,
            int pageIndex = 0,
            int pageSize = 10)
        {

            Expression<Func<Order, bool>> filter = c => (orderCode == null || c.OrderCode == orderCode)
                && (string.IsNullOrEmpty(customerName) || c.Customer.FullName.ToLower().Contains(customerName.ToLower()))
                && (string.IsNullOrEmpty(voucherCode) || c.Voucher.Code.ToLower().Contains(voucherCode.ToLower()));

            var response = await _orderService.GetListAsync(
                filter: filter,
                includeProperties: "Customer,Voucher,OrderDetails",
                pageIndex: pageIndex,
                pageSize: pageSize);

            return Ok(new ApiResult<Pagination<Order>>
            {
                Success = true,
                Result = response
            });
        }

    }
}
