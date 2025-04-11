using Microsoft.AspNetCore.Mvc;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkincareRoutineStepController : Controller
    {
        private readonly SkinCareRoutineStepService _skinCareRoutineStep;

        public SkincareRoutineStepController(SkinCareRoutineStepService skinCareRoutineStep)
        {
            _skinCareRoutineStep = skinCareRoutineStep;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllWithPagination([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _skinCareRoutineStep.GetAllPaginationAsync(page, pageSize);
                return Ok(new
                {
                    success = true,
                    message = result.message,
                    data = result.data,
                    pagination = result.pagination
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet("get-by-id/{skincareRoutineStepId}")]
        public async Task<IActionResult> GetById(int skincareRoutineStepId)
        {
            var result = await _skinCareRoutineStep.GetByIdAsync(skincareRoutineStepId);
            return result == null
                ? BadRequest(new { success = false, message = "Không tìm thấy bước skincare" })
                : Ok(new { success = true, message = "Lấy chi tiết thành công", data = result });
        }

        [HttpPost("create")]      
        public async Task<IActionResult> Create([FromBody] CreateSkinCareRoutineStepDto dto)
        {
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(kv => kv.Value.Errors.Select(e => $"{kv.Key}: {e.ErrorMessage}"))
                    .ToList();

                return BadRequest(new
                {
                    success = false,
                    message = string.Join(" | ", errors)
                });
            }

            try
            {
                var result = await _skinCareRoutineStep.CreateAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = "Tạo thành công",
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



        [HttpPut("update/{skincareRoutineStepId}")]
        public async Task<IActionResult> Update(int skincareRoutineStepId, [FromBody] UpdateSkinCareRoutineStepDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"));
                return BadRequest(new { success = false, message = string.Join(" | ", errors) });
            }

            try
            {
                var result = await _skinCareRoutineStep.UpdateAsync(skincareRoutineStepId, dto);
                return Ok(new
                {
                    success = true,
                    message = "Cập nhật thành công",
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


        [HttpDelete("delete/{skincareRoutineStepId}")]
        public async Task<IActionResult> Delete(int skincareRoutineStepId)
        {
            var deleted = await _skinCareRoutineStep.DeleteAsync(skincareRoutineStepId);
            return deleted
                ? Ok(new { success = true, message = "Xoá bước skincare thành công" })
                : BadRequest(new { success = false, message = "Không tìm thấy để xoá" });
        }
    }
}
