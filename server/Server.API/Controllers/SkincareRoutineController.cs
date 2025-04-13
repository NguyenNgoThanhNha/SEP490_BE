using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Data.Entities;

namespace Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkincareRoutineController : Controller
    {
        private readonly SkincareRoutineService _skincareRoutineService;



        public SkincareRoutineController(SkincareRoutineService skincareRoutineService)
        {
            _skincareRoutineService = skincareRoutineService;

        }

        [HttpGet("get-all")]       
        public async Task<IActionResult> GetAllWithPagination([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _skincareRoutineService.GetAllPaginationAsync(page, pageSize);
                return Ok(ApiResult<GetAllSkincareRoutinePaginationResponse>.Succeed(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi: " + ex.Message,
                }));
            }
        }


        [HttpGet("get-by-id/{skincareRoutineId}")]
        public async Task<IActionResult> GetById(int skincareRoutineId)
        {
            try
            {
                var result = await _skincareRoutineService.GetByIdAsync(skincareRoutineId);
                if (result == null)
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Không tìm thấy skincare routine",
                    }));

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Lấy thông tin skincare routine thành công",
                    data = result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi: " + ex.Message,
                }));
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateSkincareRoutineDto dto)
        {
            try
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
                
                if (dto.TotalPrice.HasValue && dto.TotalPrice <= 0)
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Giá trị tổng giá không hợp lệ.",
                    }));
                }

                var result = await _skincareRoutineService.CreateAsync(dto);
                if (result == null)
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Tao gói liệu trình thất bại.",
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Tạo gói liệu trình thành công",
                    data = result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi: " + ex.Message,
                }));
            }
        }


        [HttpPut("update/{skincareRoutineId}")]
        public async Task<IActionResult> Update(int skincareRoutineId, [FromBody] UpdateSkincareRoutineDto dto)
        {
            try
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
                
                var result = await _skincareRoutineService.UpdateAsync(skincareRoutineId, dto);
                if (result == null)
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Không tìm thấy gói liệu trình để cập nhật",
                    }));

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Cập nhật gói liệu trình thành công",
                    data = result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi: " + ex.Message,
                }));
            }
        }

        [HttpDelete("delete/{skincareRoutineId}")]
        public async Task<IActionResult> Delete(int skincareRoutineId)
        {
            try
            {
                var deleted = await _skincareRoutineService.DeleteAsync(skincareRoutineId);
                if (!deleted)
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Không tìm thấy gói liệu trình để xóa",
                    }));

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Xóa gói liệu trình thành công",
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi: " + ex.Message,
                }));
            }
        }

        [HttpGet("get-target-skin-type")]
        public async Task<IActionResult> GetTargetSkinType()
        {
            try
            {
                var result = await _skincareRoutineService.GetTargetSkinTypesAsync();
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Lấy danh sách loại da thành công",
                    data = result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi: " + ex.Message,
                }));
            }
        }
    }
}
