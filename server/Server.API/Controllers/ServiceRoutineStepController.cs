using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Dtos;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceRoutineStepController : Controller
    {
        private readonly ServiceRoutineStepService _serviceRoutineStepService;

        public ServiceRoutineStepController(ServiceRoutineStepService serviceRoutineStepService)
        {
            _serviceRoutineStepService = serviceRoutineStepService;
        }

        [HttpPost("assign-service-to-ServiceRoutineStep")]
        public async Task<IActionResult> AssignServiceToStep([FromBody] AssignServiceToRoutineStepDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"));
                return BadRequest(ApiResult<object>.Error(new ApiResponse
                {
                    message = string.Join(" | ", errors)
                }));
            }

            var result = await _serviceRoutineStepService.AssignServiceToRoutineStepAsync(dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
