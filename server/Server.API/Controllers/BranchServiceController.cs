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
    public class BranchServiceController : Controller
    {
        private readonly BranchServiceService _branchServiceservice;

        public BranchServiceController(BranchServiceService service)
        {
            _branchServiceservice = service;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _branchServiceservice.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    message = data.Any() ? "Lấy danh sách thành công" : "Danh sách rỗng",
                    data
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = true, message = $"Lỗi: {ex.Message}" });
            }
        }
        
        [HttpGet("get-all-service-in-branch/{branchId}")]
        public async Task<IActionResult> GetAllServiceInBranch(int branchId)
        {
            var data = await _branchServiceservice.GetAllServiceInBranchAsync(branchId);
            if (data == null || !data.Any())
            {
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Không có dịch vụ nào trong chi nhánh này",
                    data = new List<object>()
                }));
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy danh sách dịch vụ trong chi nhánh thành công",
                data = data
            }));
        }

        [HttpGet("get-by-id/{branchServiceId}")]
        public async Task<IActionResult> GetById(int branchServiceId)
        {
            var data = await _branchServiceservice.GetByIdAsync(branchServiceId);
            return Ok(new
            {
                success = true,
                message = data == null ? "Không tìm thấy" : "Lấy chi tiết thành công",
                data
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateBranchServiceDto dto)
        {
            var result = await _branchServiceservice.CreateAsync(dto);
            return Ok(new
            {
                success = true,
                message = result == null
                    ? "Dịch vụ này đã được gán cho chi nhánh trước đó"
                    : "Tạo thành công",
                data = result
            });
        }


        [HttpPut("update/{branchServiceId}")]
        public async Task<IActionResult> Update(int branchServiceId, [FromBody] UpdateBranchServiceDto dto)
        {
            var result = await _branchServiceservice.UpdateAsync(branchServiceId, dto);
            return Ok(new
            {
                success = true,
                message = result == null ? "Không tìm thấy để cập nhật" : "Cập nhật thành công",
                data = result
            });
        }

        [HttpDelete("delete/{branchServiceId}")]
        public async Task<IActionResult> Delete(int branchServiceId)
        {
            var deleted = await _branchServiceservice.DeleteAsync(branchServiceId);
            return Ok(new
            {
                success = true,
                message = deleted ? "Xoá thành công" : "Không tìm thấy để xoá"
            });
        }


    }
}
