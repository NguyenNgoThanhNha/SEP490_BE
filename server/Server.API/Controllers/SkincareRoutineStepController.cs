using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Response;
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
                return Ok(ApiResult<GetAllSkinCareRoutineStepPaginationResponse>.Succeed(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi: " + ex.Message,
                }));
            }
        }

        [HttpGet("get-by-id/{skincareRoutineStepId}")]
        public async Task<IActionResult> GetById(int skincareRoutineStepId)
        {
            var result = await _skinCareRoutineStep.GetDetailAsync(skincareRoutineStepId);
            return result == null
                ? BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy bước skincare",
                }))
                : Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Lấy thông tin bước skincare thành công",
                    data = result
                }));
        }

        [HttpPost("create")]      
        public async Task<IActionResult> Create([FromBody] CreateSkinCareRoutineStepDto dto)
        {
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(kv => kv.Value.Errors.Select(e => $"{kv.Key}: {e.ErrorMessage}"))
                    .ToList();

                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = errors.Count > 0 ? string.Join(" | ", errors) : "Có lỗi xảy ra",
                }));
            }

            try
            {
                var result = await _skinCareRoutineStep.CreateAsync(dto);
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = $"Tạo bước {result.Step} thành công",
                    data = result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = $"Lỗi: {ex.Message}",
                }));
            }
        }



        [HttpPut("update/{skincareRoutineStepId}")]
        public async Task<IActionResult> Update(int skincareRoutineStepId, [FromBody] UpdateSkinCareRoutineStepDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(kv => kv.Value.Errors.Select(e => $"{kv.Key}: {e.ErrorMessage}"))
                    .ToList();

                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = errors.Count > 0 ? string.Join(" | ", errors) : "Có lỗi xảy ra",
                }));
            }

            try
            {
                var result = await _skinCareRoutineStep.UpdateAsync(skincareRoutineStepId, dto);
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = $"Cập nhật bước {result.Step} thành công",
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = $"Lỗi: {ex.Message}",
                }));
            }
        }


        [HttpDelete("delete/{skincareRoutineStepId}")]
        public async Task<IActionResult> Delete(int skincareRoutineStepId)
        {
            var deleted = await _skinCareRoutineStep.DeleteAsync(skincareRoutineStepId);
            return deleted
                ? Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Xoá bước skincare thành công",
                }))
                : BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy bước skincare để xoá",
                }));
        }
    }
}
