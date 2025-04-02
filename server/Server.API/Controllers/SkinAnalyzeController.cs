using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Exceptions;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkinAnalyzeController : ControllerBase
    {
        private readonly SkinAnalyzeService _skinAnalyzeService;
        private readonly AuthService _authService;

        public SkinAnalyzeController(SkinAnalyzeService skinAnalyzeService, AuthService authService)
        {
            _skinAnalyzeService = skinAnalyzeService;
            _authService = authService;
        }
        
        [Authorize]
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeSkin(IFormFile file)
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

            // Lấy thông tin user từ token
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin khách hàng!"
                }));
            }

            try
            {
                // Gọi service AnalyzeSkinAsync
                var routines = await _skinAnalyzeService.AnalyzeSkinAsync(file, currentUser.UserId);

                return Ok(ApiResult<SkinAnalyzeResponse>.Succeed(routines));
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
                /*return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = $"Error occurred while analyzing skin with message: {ex.Message}"
                }));*/
            }
        }
        
        [Authorize]
        [HttpPost("analyze_form")]
        public async Task<IActionResult> AnalyzeSkinFromForm([FromBody]SkinHealthFormRequest request)
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

            // Lấy thông tin user từ token
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin khách hàng!"
                }));
            }

            try
            {
                // Gọi service AnalyzeSkinAsync
                var routines = await _skinAnalyzeService.AnalyzeSkinFromFormAsync(request, currentUser.UserId);

                return Ok(ApiResult<SkinAnalyzeResponse>.Succeed(routines));
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
                /*return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = $"Error occurred while analyzing skin with message: {ex.Message}"
                }));*/
            }
        }

        [Authorize]
        [HttpGet("get-skin-healths")]
        public async Task<IActionResult> GetSkinHealth()
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

            // Lấy thông tin user từ token
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin khách hàng!"
                }));
            }

            var skinHealthImages = await _skinAnalyzeService.GetSkinHealthImages(currentUser.UserId);

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy hiện trạng da thành công!",
                data = skinHealthImages
            }));
        }

    }
}
