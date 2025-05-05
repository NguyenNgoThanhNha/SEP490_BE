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
                return BadRequest("Tin nhắn không được để trống.");
            }

            var response = await _botchatService.SendChatMessageAsync(request.message);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Gửi tin nhắn thành công",
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
                    message = "Lỗi tải dữ liệu!",
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Tải dữ liệu thành công",
            }));
        }
    }
}
