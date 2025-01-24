using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Net.payOS;
using Net.payOS.Types;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Exceptions;
using Server.Business.Services;
using Server.Business.Ultils;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly PayOSSetting _payOsSetting;

        public OrderController(IOptions<PayOSSetting> payOsSetting, OrderService orderService)
        {
            _orderService = orderService;
            _payOsSetting = payOsSetting.Value;
        }
        
        [HttpPost("confirm-order-appointment")]
        public async Task<IActionResult> ConfirmOrderAppointment([FromBody] ConfirmOrderRequest req)
        {
            var checkoutUrl = await _orderService.ConfirmOrderAppointmentAsync(req);
            if(checkoutUrl == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Error in create payment link"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Create payment link successfully",
                data = checkoutUrl
            }));
        }
        
        [HttpPost("confirm-order-product")]
        public async Task<IActionResult> ConfirmOrderDetail([FromBody] ConfirmOrderRequest req)
        {
            var checkoutUrl = await _orderService.ConfirmOrderDetailAsync(req);
            if(checkoutUrl == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Error in create payment link"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Create payment link successfully",
                data = checkoutUrl
            }));
        }
        
        [HttpPost("confirm-order-deposit")]
        public async Task<IActionResult> ConfirmOrderDeposit([FromBody] DepositRequest req)
        {
            var checkoutUrl = await _orderService.DepositAppointmentAsync(req);
            if(checkoutUrl == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Error in create payment link"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Create payment link successfully",
                data = checkoutUrl
            }));
        }
    }
}
