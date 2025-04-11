using Microsoft.AspNetCore.Mvc;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Data.Entities;

namespace Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceRoutineController : Controller
    {
        private readonly ServiceRoutineService _serviceRoutineService;

        public ServiceRoutineController(ServiceRoutineService serviceRoutineService)
        {
            _serviceRoutineService = serviceRoutineService;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> Assign([FromBody] AssignServiceToRoutineDto dto)
        {           

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"));
                return BadRequest(new { success = false, message = string.Join(" | ", errors) });
            }

            try
            {
                var result = await _serviceRoutineService.AssignServiceToRoutineAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = "Gán dịch vụ vào routine thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

    }
}
