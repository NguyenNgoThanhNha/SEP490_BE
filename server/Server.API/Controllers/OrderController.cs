
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Net.payOS;
using Net.payOS.Types;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Business.Services;
using Server.Business.Ultils;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly AuthService _authService;
        private readonly PayOSSetting _payOsSetting;

        public OrderController(IOptions<PayOSSetting> payOsSetting, OrderService orderService,
            AuthService authService)
        {
            _orderService = orderService;
            _authService = authService;
            _payOsSetting = payOsSetting.Value;
        }
        
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet("get-all-order")]
        public async Task<IActionResult> GetAllOrder([FromQuery] string orderType, int page = 1, int pageSize = 5)
        {
            var orders = await _orderService.GetListOrderByOrderTypeAsync(orderType, page, pageSize);
            if (orders.Data == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Order not found!"
                }));
            }

            return Ok(ApiResult<Pagination<OrderModel>>.Succeed(orders));
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
        
        [Authorize]
        [HttpGet("history-booking")]
        public async Task<IActionResult> HistoryBooking([FromQuery] string status ,int page = 1, int pageSize = 5)
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Authorization header is missing."
                }));
            }

            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            // accessUser
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Customer info not found!"
                }));
            }
            var orders = await _orderService.BookingHistory(currentUser.UserId, status ,page, pageSize);
            if (orders.data == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "History booking not found!"
                }));
            }

            return Ok(ApiResult<HistoryBookingResponse>.Succeed(new HistoryBookingResponse()
            {
                message = orders.message,
                data = orders.data,
                pagination = orders.pagination
            }));
        }
        
        [Authorize]
        [HttpGet("detail-booking")]
        public async Task<IActionResult> DetailBooking([FromQuery] int orderId)
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Authorization header is missing."
                }));
            }

            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            // accessUser
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Customer info not found!"
                }));
            }
            var order = await _orderService.GetDetailOrder(orderId, currentUser.UserId);
            if (order == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Detail booking not found!"
                }));
            }

            return Ok(ApiResult<DetailOrderResponse>.Succeed(new DetailOrderResponse()
            {
                message = order.message,
                data = order.data
            }));
        }
        
        [Authorize]
        [HttpPost("create-order-appointment-more/{orderId}")]
        public async Task<IActionResult> CreateOrderAppointmentMore(int orderId,[FromBody] AppointmentUpdateRequest req)
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Authorization header is missing."
                }));
            }

            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            // accessUser
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Customer info not found!"
                }));
            }
            var order = await _orderService.CreateMoreOrderAppointment(orderId, req);
            if (order == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Create order appointment more failed!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Create order appointment more successfully"
            }));
        }
        
        [Authorize]
        [HttpPost("create-order-product-more/{orderId}")]
        public async Task<IActionResult> CreateOrderProductMore(int orderId,[FromBody] OrderDetailRequest req)
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Authorization header is missing."
                }));
            }

            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            // accessUser
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Customer info not found!"
                }));
            }
            var order = await _orderService.CreateMoreOrderProduct(orderId, req);
            if (order == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Create order product more failed!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Create order product more successfully"
            }));
        }

        [Authorize]
        [HttpPatch("update-order-status")]
        public async Task<IActionResult> UpdateOrderStatus([FromQuery] int orderId, string orderStatus)
        {
            var result = await _orderService.UpdateOrderStatus(orderId, orderStatus);
            if (!result)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Update order status failed!"
                }));
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Update order status successfully"
            }));
        }
        
        [Authorize]
        [HttpPatch("cancel-order/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId, string reason)
        {
            var result = await _orderService.CancelOrder(orderId, reason);
            if (!result)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Cancel order failed!"
                }));
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Cancel order successfully"
            }));
        }

        [Authorize]
        [HttpGet("get-order-by-staff")]
        public async Task<IActionResult> GetOrderByStaff([FromQuery] string status, int page = 1, int pageSize = 5)
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Authorization header is missing."
                }));
            }

            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            // accessUser
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Staff info not found!"
                }));
            }
            var orders = await _orderService.GetListAppointmentsByStaff(status, currentUser.UserId, page, pageSize);
            if (orders.Data == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Order not found!"
                }));
            }

            return Ok(ApiResult<Pagination<AppointmentsModel>>.Succeed(orders));
        }

        //[HttpPost("create-full")]
        //public async Task<IActionResult> CreateOrderWithDetails([FromBody] CreateOrderWithDetailsRequest request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ApiResult<object>.Error(null, "Invalid request format"));
        //    }

        //    var result = await _orderService.CreateOrderWithDetailsAsync(request);
        //    return Ok(result);
        //}

        [HttpPost("create-full")]
        public async Task<IActionResult> CreateOrderWithDetails([FromBody] CreateOrderWithDetailsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {                    
                    message = "Invalid request format",
                    data = null
                }));
            }
            var result = await _orderService.CreateOrderWithDetailsAsync(request);

            return Ok(result); 
        }


        [Authorize]        
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusSimpleRequest request)
        {
            if (!Request.Headers.TryGetValue("Authorization", out var tokenHeader))
            {
                return BadRequest(ApiResult<object>.Error(new ApiResponse
                {
                    message = "Authorization header is missing."
                }));
            }

            var tokenParts = tokenHeader.ToString().Split(' ');
            if (tokenParts.Length != 2 || tokenParts[0] != "Bearer")
            {
                return BadRequest(ApiResult<object>.Error(new ApiResponse
                {
                    message = "Invalid Authorization format. Expected 'Bearer <token>'"
                }));
            }

            var token = tokenParts[1];

            var result = await _orderService.UpdateOrderStatusSimpleAsync(request.OrderId, request.Status, token);

            if (!result.Success)
            {
                if (result.Result is ApiResponse errorResponse)
                {
                    return BadRequest(ApiResult<object>.Error(new ApiResponse
                    {
                        message = errorResponse.message
                    }));
                }

                return BadRequest(ApiResult<object>.Error(new ApiResponse
                {
                    message = "Unknown error occurred."
                }));
            }

            return Ok(result);
        }


        [Authorize]
        [HttpPut("update-payment-method-or-note")]
        public async Task<IActionResult> UpdatePaymentMethodOrNote([FromBody] UpdatePaymentMethodOrNoteRequest request)
        {
            // Lấy token từ header của request
            var token = Request.Headers["Authorization"].ToString().Split(' ')[1]; // "Bearer <token>"

            // Gọi Service để xử lý logic cập nhật
            var result = await _orderService.UpdatePaymentMethodOrNoteAsync(request, token);

            // Trả về kết quả từ Service
            if (!result.Success)
            {
                // Nếu không thành công, trả về lỗi với thông báo rõ ràng
                return BadRequest(ApiResult<object>.Error(new ApiResponse
                {
                    message = "Đã xảy ra lỗi trong quá trình xử lý yêu cầu. Vui lòng thử lại sau."
                }));
            }
            // Trả về thành công nếu cập nhật thành công
            return Ok(ApiResult<object>.Succeed(result.Result));
        }


    }
}
