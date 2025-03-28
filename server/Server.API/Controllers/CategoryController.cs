using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Business.Services;
using Server.Data.Entities;
using CategoryService = Server.Business.Services.CategoryService;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(string? name, string? description, int pageIndex = 0, int pageSize = 10)
        {
            var response = await _categoryService.GetListAsync(
                filter: c => (string.IsNullOrEmpty(name) || c.Name.ToLower().Contains(name.ToLower())) &&
                             (string.IsNullOrEmpty(description) || c.Description.ToLower().Contains(description.ToLower())),
                pageIndex: pageIndex,
                pageSize: pageSize);

            if (response.Data.Count == 0)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "No categories found."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Categories retrieved successfully.",
                data = response
            }));
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllCategories()
        {
            var result = await _categoryService.GetAllCategoryAsync();
            return Ok(new
            {
                success = true,
                result = new
                {
                    message = "Lấy danh sách danh mục thành công!",
                    data = result
                }
            });
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {
            try
            {
                // Gọi service để lấy dữ liệu
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);

                // Trả về kết quả thành công
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Category retrieved successfully.",
                    data = category
                }));
            }
            catch (NotFoundException ex)
            {
                // Trả về lỗi nếu không tìm thấy
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = ex.Message
                }));
            }
            catch (Exception ex)
            {
                // Trả về lỗi nếu có lỗi hệ thống
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Error: {ex.Message}"
                }));
            }
        }




        [Authorize(Roles = "Admin, Manager")]        
        [HttpPost("create")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto categoryCreateDto)
        {
            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Invalid category data.",
                    data = errors
                }));
            }

            // Gọi service để tạo danh mục
            var category = await _categoryService.CreateCategoryAsync(categoryCreateDto);

            // Kiểm tra nếu không thành công (category = null do lỗi hoặc đã tồn tại)
            if (category == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Category with the same name already exists or failed to create."
                }));
            }

            // Trả về kết quả thành công
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Category created successfully.",
                data = category
            }));
        }





        [Authorize(Roles = "Admin, Manager")]
        [HttpPut("update/{categoryId}")]
        public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] CategoryUpdateDto categoryUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Invalid category data.",
                    data = errors
                }));
            }

            try
            {
                // Gọi service để cập nhật danh mục
                var updatedCategory = await _categoryService.UpdateCategoryAsync(categoryId, categoryUpdateDto);

                // Trả về kết quả thành công
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Category updated successfully.",
                    data = updatedCategory
                }));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = ex.Message
                }));
            }
            catch (NotFoundException ex)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = ex.Message
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Internal server error: {ex.Message}"
                }));
            }
        }



        [Authorize(Roles = "Admin, Manager")]
        [HttpDelete("delete/{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                // Gọi service để xử lý logic
                var updatedCategory = await _categoryService.DeleteCategoryAsync(categoryId);

                // Trả về kết quả thành công
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Category status updated to Inactive successfully.",
                    data = updatedCategory
                }));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = ex.Message
                }));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = ex.Message
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Internal server error: {ex.Message}"
                }));
            }
        }
    }
}
