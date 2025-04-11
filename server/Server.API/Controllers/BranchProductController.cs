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
    public class BranchProductController : Controller
    {
        private readonly BranchProductService _branchProductservice;

        public BranchProductController(BranchProductService service)
        {
            _branchProductservice = service;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _branchProductservice.GetAllAsync();

            if (result == null || !result.Any())
            {
                return Ok(new
                {
                    success = true,
                    message = "Không có sản phẩm chi nhánh nào.",
                    data = new List<object>()
                });
            }

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách sản phẩm chi nhánh thành công",
                data = result
            });
        }

        [HttpGet("get-all-product-in-branch/{branchId}")]
        public async Task<IActionResult> GetAllProductInBranch(int branchId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _branchProductservice.GetAllProductInBranchPaginationAsync(branchId, page, pageSize);

            if (result == null || result.data == null || !result.data.Any())
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không có sản phẩm nào trong chi nhánh này"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy danh sách sản phẩm trong chi nhánh thành công",
                data = result
            }));
        }


        [HttpGet("get-by-id/{ProductbranchId}")]
        public async Task<IActionResult> GetById(int ProductbranchId)
        {
            var result = await _branchProductservice.GetByIdAsync(ProductbranchId);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateBranchProductDto dto)
        {
            // Validate đầu vào
            if (dto.StockQuantity <= 0)
            {
                return Ok(new
                {
                    success = false,
                    message = "Số lượng tồn kho phải lớn hơn 0"
                });
            }

            var product = await _branchProductservice.CheckProductExist(dto.ProductId);
            if (product == null)
            {
                return Ok(new
                {
                    success = false,
                    message = $"Không tìm thấy sản phẩm với ID = {dto.ProductId}"
                });
            }

            var branch = await _branchProductservice.CheckBranchExist(dto.BranchId);
            if (branch == null)
            {
                return Ok(new
                {
                    success = true,
                    message = $"Không tìm thấy chi nhánh với ID = {dto.BranchId}"
                });
            }

            var result = await _branchProductservice.CreateAsync(dto);

            return Ok(new
            {
                success = true,
                message = "Tạo sản phẩm chi nhánh thành công",
                data = result
            });
        }


        [HttpPut("update/{ProductbranchId}")]
        public async Task<IActionResult> Update(int ProductbranchId, UpdateBranchProductDto dto)
        {
            var updated = await _branchProductservice.UpdateAsync(ProductbranchId, dto);

            if (!updated)
            {
                return Ok(new
                {
                    success = false,
                    message = $"Không tìm thấy sản phẩm chi nhánh với ID = {ProductbranchId}"
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Cập nhật sản phẩm chi nhánh ID = {ProductbranchId} thành công"
            });
        }

        [HttpDelete("delete/{ProductbranchId}")]
        public async Task<IActionResult> Delete(int ProductbranchId)
        {
            var deleted = await _branchProductservice.DeleteAsync(ProductbranchId);

            if (!deleted)
            {
                return Ok(new
                {
                    success = false,
                    message = $"Không tìm thấy sản phẩm chi nhánh với ID = {ProductbranchId}"
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Xóa sản phẩm chi nhánh ID = {ProductbranchId} thành công"
            });
        }


    }
}
