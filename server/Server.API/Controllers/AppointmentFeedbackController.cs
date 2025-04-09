using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Services;
using Server.Data.UnitOfWorks;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentFeedbackController : Controller
    {
        private readonly AppointmentFeedbackService _appointmentFeedbackService;      
      


        public AppointmentFeedbackController(AppointmentFeedbackService appointmentFeedbackService)
        {
            _appointmentFeedbackService = appointmentFeedbackService;         
           
        }

        [HttpGet("get-by-id{appointmentFeedbackId}")]
        public async Task<IActionResult> GetById(int appointmentFeedbackId)
        {
            try
            {
                var result = await _appointmentFeedbackService.GetByIdAsync(appointmentFeedbackId);

                if (result == null)
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Không tìm thấy phản hồi với ID được cung cấp.",
                        data = new List<object>()
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Lấy phản hồi thành công!",
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
