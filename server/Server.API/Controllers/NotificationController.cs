using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationServices _notificationServices;

        public NotificationController(NotificationServices notificationServices)
        {
            _notificationServices = notificationServices;
        }
        
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllNotificationsByUserId([FromQuery] int userId, int pageIndex, int pageSize)
        {
            try
            {
                var result = await _notificationServices.GetAllNotificationsByUserIdAsync(userId, pageIndex, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = ex.Message,
                }));
            }
        }
        
        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] int notificationId)
        {
            try
            {
                var result = await _notificationServices.MarkAsReadAsync(notificationId);
                if (!result)
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Không tìm thấy thông báo",
                    }));
                }
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Đánh dấu đã đọc thành công",
                    data = result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = ex.Message,
                }));
            }
        }
    }
}
