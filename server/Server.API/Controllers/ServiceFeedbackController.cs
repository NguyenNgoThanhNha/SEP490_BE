using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceFeedbackController : Controller
    {
        private readonly ServiceFeedbackService _serviceFeedbackService;



        public ServiceFeedbackController(ServiceFeedbackService serviceFeedbackService)
        {
            _serviceFeedbackService = serviceFeedbackService;

        }

        [HttpGet("get-by-id/{serviceFeedbackId}")]
        public async Task<IActionResult> GetById(int serviceFeedbackId)
        {
            try
            {
                var result = await _serviceFeedbackService.GetByIdAsync(serviceFeedbackId);

                if (result == null)
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Không tìm thấy phản hồi dịch vụ với ID được cung cấp.",
                        data = new List<object>()
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Lấy phản hồi dịch vụ thành công!",
                    data = result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi hệ thống: {ex.Message}",
                    data = new List<object>()
                }));
            }
        }
    }
}
