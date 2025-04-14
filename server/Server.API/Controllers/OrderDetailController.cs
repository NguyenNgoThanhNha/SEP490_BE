using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Services;
using Microsoft.AspNet.SignalR;

namespace Server.API.Controllers
{
     [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : Controller
    {
        private readonly OrderDetailService _orderDetailService;
        

        public OrderDetailController(OrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;          
        }

        [Authorize]
        [HttpGet("get-by-branchId")]
        public async Task<IActionResult> GetByBranchPaged([FromQuery] int branchId, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var result = await _orderDetailService.GetOrderDetailsByBranchIdAsync(branchId, page, pageSize);

            if (result == null || result.data == null || !result.data.Any())
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không tìm thấy chi tiết đơn hàng theo chi nhánh."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Lấy danh sách chi tiết đơn hàng theo chi nhánh thành công!",
                data = result
            }));
        }


    }
}
