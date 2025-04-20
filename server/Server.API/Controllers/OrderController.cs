
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

        [Authorize(Roles = "Admin, Manager, Staff")]
        [HttpGet("get-all-order")]
        public async Task<IActionResult> GetAllOrder([FromQuery] GetAllOrderRequest request)
        {
            var orders = await _orderService.GetListOrderFilterAsync(request);
            if (orders.data == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy đơn hàng!"
                }));
            }

            return Ok(ApiResult<GetListOrderFilterResponse>.Succeed(orders));
        }

        #region old confirm order

        /*[HttpPost("confirm-order-appointment")]
public async Task<IActionResult> ConfirmOrderAppointment([FromBody] ConfirmOrderRequest req)
{
    var checkoutUrl = await _orderService.ConfirmOrderAppointmentAsync(req);
    if (checkoutUrl == null)
    {
        return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
        {
            message = "Lỗi khi tạo liên kết thanh toán"
        }));
    }
    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
    {
        message = "Tạo liên kết thanh toán thành công",
        data = checkoutUrl
    }));
}

[HttpPost("confirm-order-product")]
public async Task<IActionResult> ConfirmOrderDetail([FromBody] ConfirmOrderRequest req)
{
    var checkoutUrl = await _orderService.ConfirmOrderDetailAsync(req);
    if (checkoutUrl == null)
    {
        return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
        {
            message = "Lỗi khi tạo liên kết thanh toán"
        }));
    }
    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
    {
        message = "Tạo liên kết thanh toán thành công",
        data = checkoutUrl
    }));
}*/

        #endregion
        
        [HttpPost("confirm-order")]
        public async Task<IActionResult> ConfirmOrder([FromBody] ConfirmOrderRequest req)
        {
            var checkoutUrl = await _orderService.ConfirmOrderAsync(req);
            if (checkoutUrl == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi khi tạo liên kết thanh toán"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Tạo liên kết thanh toán thành công",
                data = checkoutUrl
            }));
        }
        

        [HttpPost("confirm-order-deposit")]
        public async Task<IActionResult> ConfirmOrderDeposit([FromBody] DepositRequest req)
        {
            var checkoutUrl = await _orderService.DepositAppointmentAsync(req);
            if (checkoutUrl == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi khi tạo liên kết thanh toán"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Tạo liên kết thanh toán thành công",
                data = checkoutUrl
            }));
        }

        [Authorize]
        [HttpGet("history-booking")]
        public async Task<IActionResult> HistoryBooking([FromQuery] string status, string orderType, int page = 1, int pageSize = 5)
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
                    message = "Không tìm thấy thông tin khách hàng!"
                }));
            }
            var orders = await _orderService.BookingHistory(currentUser.UserId, status, orderType, page, pageSize);
            if (orders.data == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy lịch sử lịch hẹn!"
                }));
            }

            return Ok(ApiResult<HistoryBookingResponse>.Succeed(new HistoryBookingResponse()
            {
                message = orders.message,
                data = orders.data,
                pagination = orders.pagination
            }));
        }

        [HttpGet("history-booking-all")]
        public async Task<IActionResult> HistoryBookingAllTypes([FromQuery] string status, int page = 1, int pageSize = 5)
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
            // Lấy thông tin người dùng từ token
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin khách hàng!"
                }));
            }

            var orders = await _orderService.BookingHistoryAllTypes(currentUser.UserId, status, page, pageSize);

            // Trường hợp tìm thấy nhưng không có đơn hàng nào
            if (orders.data == null || !orders.data.Any())
            {
                return Ok(ApiResult<HistoryBookingResponse>.Succeed(new HistoryBookingResponse()
                {
                    message = "Không có lịch sử lịch hẹn nào.",
                    data = new List<OrderModel>(),
                    pagination = orders.pagination
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
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Authorization header is missing."
                }));
            }

            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin khách hàng!"
                }));
            }

            var order = await _orderService.GetDetailOrder(orderId, currentUser.UserId);

            if (order == null || order.data == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin lịch hẹn chi tiết!"
                }));
            }

            return Ok(ApiResult<DetailOrderResponse>.Succeed(order));
        }




        [Authorize]
        [HttpPost("create-order-appointment-more/{orderId}")]
        public async Task<IActionResult> CreateOrderAppointmentMore(int orderId, [FromBody] AppointmentUpdateRequest req)
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
                    message = "Không tìm thấy thông tin khách hàng!"
                }));
            }
            var order = await _orderService.CreateMoreOrderAppointment(orderId, req);
            if (order == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Tạo thêm lịch hẹn không thành công!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Tạo thêm lịch hẹn thành công!"
            }));
        }

        [Authorize]
        [HttpPost("create-order-product-more/{orderId}")]
        public async Task<IActionResult> CreateOrderProductMore(int orderId, [FromBody] OrderDetailRequest req)
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
                    message = "Không tìm thấy thông tin khách hàng!"
                }));
            }
            var order = await _orderService.CreateMoreOrderProduct(orderId, req);
            if (order == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Tạo thêm đơn hàng sản phẩm không thành công!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Tạo thêm đơn hàng sản phẩm thành công!"
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
                    message = "Cập nhật trạng thái đơn hàng không thành công!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Cập nhật trạng thái đơn hàng thành công"
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
                    message = "Hủy đơn hàng không thành công!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Hủy đơn hàng thành công!"
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
                    message = "Không tìm thấy thông tin nhân viên!"
                }));
            }
            var orders = await _orderService.GetListAppointmentsByStaff(status, currentUser.UserId, page, pageSize);
            if (orders.Data == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy đơn hàng!"
                }));
            }

            return Ok(ApiResult<Pagination<AppointmentsModel>>.Succeed(orders));
        }


       
      
      
        [HttpPost("create-full")]
        public async Task<IActionResult> CreateOrderWithDetails([FromBody] CreateOrderWithDetailsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResult<object>.Error(new ApiResponse
                {
                    message = "Định dạng yêu cầu không hợp lệ"
                }));
            }

            var result = await _orderService.CreateOrderWithDetailsAsync(request);

            // ❗ Nếu tạo thất bại
            if (!result.Success)
            {
                return BadRequest(ApiResult<object>.Error(new ApiResponse
                {
                    message = (result.Result as ApiResponse)?.message ?? "Tạo đơn hàng thất bại."
                }));
            }

            // ✅ Nếu thành công
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = (result.Result as ApiResponse)?.message,
                data = (result.Result as ApiResponse)?.data
            }));
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
                    message = "Đã xảy ra lỗi không xác định."
                }));
            }

            return Ok(result);
        }


        [Authorize]
        [HttpPut("update-payment-method-or-note")]
        public async Task<IActionResult> UpdatePaymentMethodOrNote([FromBody] UpdatePaymentMethodOrNoteRequest request)
        {
            // Gọi Service để xử lý logic cập nhật
            var result = await _orderService.UpdatePaymentMethodOrNoteAsync(request);

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

      
        [HttpGet("routine-history")]
        public async Task<IActionResult> GetRoutineHistory(int userId)
        {
            var result = await _orderService.GetRoutineHistoryByCustomerIdAsync(userId);

            if (result == null || !result.Any())
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không có lịch sử gói liệu trình nào được tìm thấy."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Lấy lịch sử gói liệu trình thành công!",
                data = result
            }));
        }

        [HttpPut("update-order-details-status")]
        public async Task<IActionResult> UpdateOrderDetailsStatus([FromBody] UpdateOrderDetailsStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Định dạng yêu cầu không hợp lệ"
                }));
            }

            var result = await _orderService.UpdateOrderDetailStatus(request.OrderDetailsIds, request.Status);

            if (!result)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Cập nhật trạng thái chi tiết đơn hàng thất bại."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Cập nhật trạng thái chi tiết đơn hàng thành công.",
            }));
        }
    }
}
