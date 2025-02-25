using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkScheduleController : ControllerBase
    {
        private readonly WorkScheduleService _workScheduleService;

        public WorkScheduleController(WorkScheduleService workScheduleService)
        {
            _workScheduleService = workScheduleService;
        }
        
        [Authorize]
        [HttpPost("create-work-schedule")]
        public async Task<IActionResult> CreateWorkScheduleAsync(WorkSheduleRequest workSheduleRequest)
        {
                await _workScheduleService.CreateWorkScheduleAsync(workSheduleRequest);
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Create work schedule successfully",
                }));
        }

    }
}
