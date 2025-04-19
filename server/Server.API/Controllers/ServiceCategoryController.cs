using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCategoryController : ControllerBase
    {
        private readonly ServiceCategoryService _serviceCategoryService;

        public ServiceCategoryController(ServiceCategoryService serviceCategoryService)
        {
            _serviceCategoryService = serviceCategoryService;
        }

        // Create a new ServiceCategory
        [HttpPost("create")]
        public async Task<IActionResult> CreateServiceCategory([FromForm] ServiceCategoryCreateUpdateDto dto)
        {
            var result = await _serviceCategoryService.CreateServiceCategoryAsync(dto);
            if (result != null)
            {
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    data = result,
                    message = "Tạo danh mục dịch vụ thành công"
                }));
            }
            return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Tạo danh mục dịch vụ thất bại"
            }));
        }

        // Get a ServiceCategory by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceCategoryById(int id)
        {
            var result = await _serviceCategoryService.GetServiceCategoryByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy danh mục dịch vụ"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                data = result,
                message = "Lấy danh mục dịch vụ thành công"
            }));
        }

        // Search ServiceCategories by Name
        [HttpGet("search")]
        public async Task<IActionResult> SearchServiceCategories([FromQuery] string? keyword, int page = 1, int pageSize = 5)
        {
            var result = await _serviceCategoryService.SearchServiceCategoriesAsync(keyword, page, pageSize);
            result.message = "Lấy danh mục dịch vụ thành công";
            return Ok(ApiResult<GetAllServiceCategoryResponse>.Succeed(result));
        }

        // Update an existing ServiceCategory
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServiceCategory(int id, [FromForm] ServiceCategoryCreateUpdateDto dto)
        {
            var result = await _serviceCategoryService.UpdateServiceCategoryAsync(id, dto);
            if (result == null)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy danh mục dịch vụ"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                data = result,
                message = "Cập nhật danh mục dịch vụ thành công"
            }));
        }

        // Delete a ServiceCategory
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceCategory(int id)
        {
            var success = await _serviceCategoryService.DeleteServiceCategoryAsync(id);
            if (!success)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy danh mục dịch vụ"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Xóa danh mục dịch vụ thành công"
            }));
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllServiceCategories()
        {
            var result = await _serviceCategoryService.GetAllServiceCategoriesAsync();
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                data = result,
                message = "Lấy danh sách danh mục dịch vụ thành công"
            }));
        }

    }

}
