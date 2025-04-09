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
    public class RoutineController : ControllerBase
    {
        private readonly RoutineService _routineService;

        public RoutineController(RoutineService routineService)
        {
            _routineService = routineService;
        }
        
        [HttpGet("get-list-skincare-routines")]
        public async Task<IActionResult> GetListSkincareRoutines()
        {
            var routines = await _routineService.GetListSkincareRoutine();
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy danh sách liệu trình thành công!",
                data = routines
            }));
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSkincareRoutineDetails(int id)
        {
            var routine = await _routineService.GetSkincareRoutineDetails(id);
            if (routine == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Không tìm thấy liệu trình!"
            }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công chi tiết liệu trình!",
                data = routine
            }));
        }
        
        [HttpGet("get-list-skincare-routines-step/{routineId}")]
        public async Task<IActionResult> GetListSkincareRoutineSteps(int routineId)
        {
            var steps = await _routineService.GetListSkincareRoutineStepByRoutineId(routineId);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công các bước của liệu trình!",
                data = steps
            }));
        }
        
        [HttpGet("get-list-routine-by/{userId}/{status}")]
        public async Task<IActionResult> GetListSkincareRoutineByUserId(int userId, string status)
        {
            var routines = await _routineService.GetListSkincareRoutineByUserId(userId, status);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công liệu trình!",
                data = routines
            }));
        }
        
        [HttpPost("book-compo-skin-care-routine")]
        public async Task<IActionResult> BookCompoSkinCareRoutine(BookCompoSkinCareRoutineRequest request)
        {
            var result = await _routineService.BookCompoSkinCareRoutine(request);
            if (result == 0) return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Đặt liệu trình thất bại!"
            }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Đặt liệu trình thành công!",
                data = result
            }));
        }
        
        [HttpGet("tracking-user-routine/{routineId}/{userId}")]
        public async Task<IActionResult> TrackingUserRoutine(int routineId, int userId)
        {
            var routine = await _routineService.TrackingUserRoutineByRoutineId(routineId, userId);
            if (routine == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Không tìm thấy liệu trình!"
            }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công chi tiết lịệu trình!",
                data = routine
            }));
        }
        
        [HttpGet("get-routine-of-user-newest/{userId}")]
        public async Task<IActionResult> GetRoutineOfUserNewest(int userId)
        {
            var routine = await _routineService.GetInfoRoutineOfUserNew(userId);
            if (routine == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Không tìm thấy liệu trình!"
            }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công chi tiết liệu trình!",
                data = routine
            }));
        }
        
        [HttpGet("get-detail-order-routine/{orderId}/{userId}")]
        public async Task<IActionResult> GetDetailOrderRoutine(int orderId, int userId)
        {
            var routine = await _routineService.GetDetailOrderRoutine(userId, orderId);
            if (routine == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Không tìm thấy đơn hàng"
            }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công chi tiết đơn hàng!",
                data = routine
            }));
        }
    }
}
