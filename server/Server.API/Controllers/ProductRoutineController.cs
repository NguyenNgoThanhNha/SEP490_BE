using Microsoft.AspNetCore.Mvc;
using Server.Business.Dtos;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductRoutineController : Controller
    {
        private readonly ProductRoutineService _productRoutineService;

        public ProductRoutineController(ProductRoutineService productRoutineService)
        {
            _productRoutineService = productRoutineService;
        }

        [HttpPost("assign-product-to-productRoutine")]
        public async Task<IActionResult> AssignProductToRoutine([FromBody] AssignProductToRoutineDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"));
                return BadRequest(new { success = false, message = string.Join(" | ", errors) });
            }

            try
            {
                var result = await _productRoutineService.AssignProductToRoutineAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = "Gán sản phẩm vào product routine thành công",
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
