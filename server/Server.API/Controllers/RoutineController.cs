using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
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
                message = "Routines fetched successfully",
                data = routines
            }));
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSkincareRoutineDetails(int id)
        {
            var routine = await _routineService.GetSkincareRoutineDetails(id);
            if (routine == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Routine not found"
            }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Routine details fetched successfully",
                data = routine
            }));
        }
        
        [HttpGet("get-list-skincare-routines-step/{routineId}")]
        public async Task<IActionResult> GetListSkincareRoutineSteps(int routineId)
        {
            var steps = await _routineService.GetListSkincareRoutineStepByRoutineId(routineId);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Routine steps fetched successfully",
                data = steps
            }));
        }
    }
}
