using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Data.Entities;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;
        private readonly AppDbContext _context;
        public CategoryController(CategoryService categoryService, AppDbContext context)
        {
            _categoryService = categoryService;
            _context = context;
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(string? name, string? description, int pageIndex = 0, int pageSize = 10)
        {
            var response = await _categoryService.GetListAsync(
                filter: c => (string.IsNullOrEmpty(name) || c.Name.ToLower().Contains(name.ToLower()))
                && (string.IsNullOrEmpty(description) || c.Description.ToLower().Contains(description.ToLower())),
                pageIndex: pageIndex,
                pageSize: pageSize);

            return Ok(ApiResponse.Succeed(response));
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto categoryCreateDto)
        {
            if (categoryCreateDto == null)
            {
                return BadRequest(ApiResponse.Error("Invalid category data."));
            }

            try
            {
                var result = await _categoryService.CreateCategoryAsync(categoryCreateDto);
                if (result.message == null)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, ApiResponse.Error($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {

            var category = await _categoryService.GetCategoryByIdAsync(categoryId);

            if (category == null)
            {

                return NotFound(ApiResponse.Error($"Category with ID {categoryId} not found."));
            }


            var categoryDto = new CategoryDetailDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                SkinTypeSuitable = category.SkinTypeSuitable,
                Status = category.Status,
                ImageUrl = category.ImageUrl,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate
            };

            return Ok(ApiResponse.Succeed(categoryDto));
        }

        [HttpGet("get-all-categories")]
        public async Task<IActionResult> Get([FromQuery] int page = 1)
        {
            var categories = await _categoryService.GetAllCategory(page);
            return Ok(ApiResponse.Succeed(new GetAllCategoryPaginationResponse()
            {
                data = categories.data,
                pagination = categories.pagination
            }));
        }


        [HttpPut("update{categoryId}")]
        public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] CategoryUpdateDto categoryUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Error("Invalid model state"));
            }


            var result = await _categoryService.UpdateCategoryAsync(categoryId, categoryUpdateDto);


            if (result.message != null)
            {
                return BadRequest(ApiResponse.Error(result.message));
            }


            dynamic category = result.data;
            var categoryDetailDto = new CategoryDetailDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                SkinTypeSuitable = category.SkinTypeSuitable,
                Status = category.Status,
                ImageUrl = category.ImageUrl,
                UpdatedDate = category.UpdatedDate
            };


            return Ok(ApiResponse.Succeed(categoryDetailDto));
        }

        [HttpDelete("delete/{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {

                var result = await _categoryService.DeleteCategoryAsync(categoryId);


                if (result.message != null)
                {
                    return BadRequest(result);
                }


                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {

                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }


    }
}
