using Microsoft.AspNetCore.Mvc;
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
                return Ok(new { success = true, message = result.message, data = result.data, pagination = result.pagination });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }


        [HttpGet("get-by-id/{skincareRoutineId}")]
        public async Task<IActionResult> GetById(int skincareRoutineId)
        {
            try
            {
                var result = await _skincareRoutineService.GetByIdAsync(skincareRoutineId);
                if (result == null)
                    return BadRequest(new { success = false, message = "Không tìm thấy skincare routine" });

                return Ok(new { success = true, message = "Lấy chi tiết thành công", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateSkincareRoutineDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tên quy trình (Name) là bắt buộc."
                    });
                }

                if (dto.TotalPrice.HasValue && dto.TotalPrice <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tổng giá (TotalPrice) phải lớn hơn 0."
                    });
                }

                var result = await _skincareRoutineService.CreateAsync(dto);
                if (result == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tạo skincare routine thất bại."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Tạo skincare routine thành công.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Lỗi: {ex.Message}"
                });
            }
        }


        [HttpPut("update/{skincareRoutineId}")]
        public async Task<IActionResult> Update(int skincareRoutineId, [FromBody] UpdateSkincareRoutineDto dto)
        {
            try
            {
                var result = await _skincareRoutineService.UpdateAsync(skincareRoutineId, dto);
                if (result == null)
                    return BadRequest(new { success = false, message = "Không tìm thấy skincare routine để cập nhật" });

                return Ok(new { success = true, message = "Cập nhật thành công", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpDelete("delete/{skincareRoutineId}")]
        public async Task<IActionResult> Delete(int skincareRoutineId)
        {
            try
            {
                var deleted = await _skincareRoutineService.DeleteAsync(skincareRoutineId);
                if (!deleted)
                    return BadRequest(new { success = false, message = "Không tìm thấy skincare routine để xoá" });

                return Ok(new { success = true, message = "Xoá skincare routine thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
