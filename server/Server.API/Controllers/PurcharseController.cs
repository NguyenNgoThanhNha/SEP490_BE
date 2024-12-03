using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.API.Controllers.Gaurd;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Data.Entities;
using System.Linq.Expressions;

namespace Server.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PurcharseController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly OrderDetailService _orderDetailService;
        private readonly AppDbContext _context;
        public PurcharseController(OrderService orderService,
            OrderDetailService orderDetailService,
            AppDbContext context)
        {
            _orderService = orderService;
            _context = context;
            _orderDetailService = orderDetailService;
        }

        [CustomAuthorize("Admin,Manager,Staff")]
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

            return Ok(ApiResponse.Succeed(response));
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrderAsync(CUOrderDto model)
        {
            if (ModelState.IsValid)
            {
                model.OrderId = 0;
                var order = await _orderService.CreateOrderAsync(model);
                if (order.message == null)
                    return Ok(order);
                return BadRequest(order);
            }
            return BadRequest(ApiResponse.Error("Vui lòng nhập đầy đủ thông tin"));
        }

        [HttpPost("create-order-detail")]
        public async Task<IActionResult> CreateOrderDetailAsync(CUOrderDetailDto model)
        {
            if (ModelState.IsValid)
            {
                model.OrderDetailId = 0;
                var order = await _orderDetailService.CreateOrderDetailAsync(model);
                if (order.Success)
                    return Ok(order);
                return BadRequest(order);
            }
            return BadRequest(ApiResponse.Error("Vui lòng nhập đầy đủ thông tin"));
        }
    }
}
