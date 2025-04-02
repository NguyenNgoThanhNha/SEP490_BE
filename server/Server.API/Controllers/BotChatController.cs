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
    public class BotChatController : ControllerBase
    {
        private readonly BotchatService _botchatService;

        public BotChatController(BotchatService botchatService)
        {
            _botchatService = botchatService;
        }
        
        [HttpPost("send")]
        public async Task<IActionResult> SendChat([FromBody] BotchatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.message))
            {
                return BadRequest("Tin nh?n không ???c ?? tr?ng.");
            }

            var response = await _botchatService.SendChatMessageAsync(request.message);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "G?i tin nh?n thành công",
                data = response
            }));
        }
        
        [HttpGet("seed")]
        public async Task<IActionResult> SeedingDataChat()
        {
            var response = await _botchatService.SeedingDataChatbot();
            if (!response)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "L?i t?o d? li?u!",
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "T?o d? li?u thành công",
            }));
        }
    }
}
