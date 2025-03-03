using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly FeedbackService _feedbackService;

        public FeedbackController(FeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        
        [Authorize]
        [HttpPost("service")]
        public async Task<IActionResult> CreateFeedbackService([FromBody] ServiceFeedbackRequest feedback)
        {
            var result = await _feedbackService.CreateFeedbackService(feedback);
            if(result.Success == false)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = result.Message
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = result.Message
            }));
        }
        
        [Authorize]
        [HttpPost("appointment")]
        public async Task<IActionResult> CreateFeedbackAppointment([FromBody] AppointmentFeedbackRequest feedback)
        {
            var result = await _feedbackService.CreateFeedbackAppointment(feedback);
            if(result.Success == false)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = result.Message
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = result.Message
            }));
        }
    }
}
