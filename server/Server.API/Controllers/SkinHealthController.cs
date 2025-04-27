using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkinHealthController : ControllerBase
    {
        private readonly SkinHealthService _skinHealthService;

        public SkinHealthController(SkinHealthService skinHealthService)
        {
            _skinHealthService = skinHealthService;
        }
        
        [Authorize]
        [HttpGet("get-my-skin-health/{userId}")]
        public async Task<IActionResult> GetMySkinHealth(int userId)
        {
            var skinHealthData = await _skinHealthService.GetSkinHealthDataAsync(userId);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Get skin health data successfully",
                data = skinHealthData
            }));
        }
    }
}
