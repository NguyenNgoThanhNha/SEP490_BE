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
    public class UserRoutineLoggerController : ControllerBase
    {
        private readonly UserRoutineLoggerService _userRoutineLoggerService;

        public UserRoutineLoggerController(UserRoutineLoggerService userRoutineLoggerService)
        {
            _userRoutineLoggerService = userRoutineLoggerService;
        }
        
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllUserRoutineLoggers()
        {
            try
            {
                var result = await _userRoutineLoggerService.GetAllUserRoutineLoggersAsync();
                
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Lấy dữ liệu thành công",
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

        [HttpPost("create")]
        public async Task<IActionResult> CreateUserRoutineLogger([FromBody] UserRoutineLoggerRequest request)
        {
            try
            {
                var result = await _userRoutineLoggerService.CreateUserRoutineLogger(request);
                if (!result)
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Thêm mới không thành công",
                        data = result
                    }));
                }
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Thêm mới thành công",
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

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUserRoutineLogger(int id, [FromBody] UserRoutineLoggerRequest request)
        {
            try
            {
                var result = await _userRoutineLoggerService.UpdateUserRoutineLoggerAsync(id,request);
                if (!result)
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Cập nhật không thành công",
                        data = result
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Cập nhật thành công",
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
        
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUserRoutineLogger(int id)
        {
            try
            {
                var result = await _userRoutineLoggerService.DeleteUserRoutineLoggerAsync(id);
                if (!result)
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Xóa không thành công",
                        data = result
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Xóa thành công",
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
