using Microsoft.AspNetCore.Mvc;
using Server.Business.Dtos;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductRoutineStepController : Controller
    {
        private readonly ProductRoutineStepService _productRoutineStepService;

        public ProductRoutineStepController(ProductRoutineStepService productRoutineStepService)
        {
            _productRoutineStepService = productRoutineStepService;
        }

        [HttpPost("assign-product-to-ProductRoutineStep")]
        public async Task<IActionResult> AssignProductToStep([FromBody] AssignProductToProductRoutineStepDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"));
                return BadRequest(new { success = false, message = string.Join(" | ", errors) });
            }

            try
            {
                var result = await _productRoutineStepService.AssignProductToRoutineStepAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = "Gán sản phẩm vào bước skincare thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

    }
}
