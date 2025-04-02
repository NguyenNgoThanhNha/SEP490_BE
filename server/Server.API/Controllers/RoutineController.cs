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
                message = "L?y danh s�ch li?u tr�nh th�nh c�ng!",
                data = routines
            }));
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSkincareRoutineDetails(int id)
        {
            var routine = await _routineService.GetSkincareRoutineDetails(id);
            if (routine == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Kh�ng t�m th?y li?u tr�nh!"
            }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "L?y th�nh c�ng chi ti?t li?u tr�nh!",
                data = routine
            }));
        }
        
        [HttpGet("get-list-skincare-routines-step/{routineId}")]
        public async Task<IActionResult> GetListSkincareRoutineSteps(int routineId)
        {
            var steps = await _routineService.GetListSkincareRoutineStepByRoutineId(routineId);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "L?y th�nh c�ng c�c b??c c?a li?u tr�nh!",
                data = steps
            }));
        }
        
        [HttpGet("get-list-routine-by/{userId}/{status}")]
        public async Task<IActionResult> GetListSkincareRoutineByUserId(int userId, string status)
        {
            var routines = await _routineService.GetListSkincareRoutineByUserId(userId, status);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "L?y th�nh c�ng li?u tr�nh!",
                data = routines
            }));
        }
        
        [HttpPost("book-compo-skin-care-routine")]
        public async Task<IActionResult> BookCompoSkinCareRoutine(BookCompoSkinCareRoutineRequest request)
        {
            var result = await _routineService.BookCompoSkinCareRoutine(request);
            if (result == 0) return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "??t li?u tr�nh th?t b?i!"
            }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "??t li?u tr�nh th�nh c�ng!",
                data = result
            }));
        }
        
        [HttpGet("tracking-user-routine/{routineId}/{userId}")]
        public async Task<IActionResult> TrackingUserRoutine(int routineId, int userId)
        {
            var routine = await _routineService.TrackingUserRoutineByRoutineId(routineId, userId);
            if (routine == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Kh�ng t�m th?y li?u tr�nh!"
            }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "L?y th�nh c�ng chi ti?t li?u tr�nh!",
                data = routine
            }));
        }
        
        [HttpGet("get-routine-of-user-newest/{userId}")]
        public async Task<IActionResult> GetRoutineOfUserNewest(int userId)
        {
            var routine = await _routineService.GetInfoRoutineOfUserNew(userId);
            if (routine == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Kh�ng t�m th?y li?u tr�nh!"
            }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "L?y th�nh c�ng chi ti?t li?u tr�nh!",
                data = routine
            }));
        }
        
        [HttpGet("get-detail-order-routine/{orderId}/{userId}")]
        public async Task<IActionResult> GetDetailOrderRoutine(int orderId, int userId)
        {
            var routine = await _routineService.GetDetailOrderRoutine(userId, orderId);
            if (routine == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Kh�ng t�m th?y ??n h�ng"
            }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "L?y th�nh c�ng chi ti?t ??n h�ng!",
                data = routine
            }));
        }
    }
}
