using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Services;
using Server.Business.Dtos;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductFeedbackController : Controller
    {
        private readonly ProductFeedbackService _productFeedbackService;
        public ProductFeedbackController(ProductFeedbackService productFeedbackService)
        {
            _productFeedbackService = productFeedbackService;

        }

        [HttpGet("get-by-id/{productFeedbackId}")]
        public async Task<IActionResult> GetById(int productFeedbackId)
        {
            try
            {
                var result = await _productFeedbackService.GetByIdAsync(productFeedbackId);

                if (result == null)
                {
                    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                    {
                        message = "Không tìm thấy phản hồi sản phẩm với ID được cung cấp.",
                        data = new List<object>()
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Lấy phản hồi sản phẩm thành công!",
                    data = result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi hệ thống: {ex.Message}",
                    data = new List<object>()
                }));
            }
        }

        [HttpGet("get-by-product/{productId}")]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            try
            {
                var result = await _productFeedbackService.GetByProductIdAsync(productId);

                if (result == null || !result.Any())
                {
                    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                    {
                        message = "Không tìm thấy phản hồi nào cho sản phẩm này.",
                        data = new List<object>()
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Lấy danh sách phản hồi sản phẩm theo ProductId thành công!",
                    data = result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi hệ thống: {ex.Message}",
                    data = new List<object>()
                }));
            }
        }


        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _productFeedbackService.GetAllAsync();
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Lấy danh sách phản hồi sản phẩm thành công!",
                data = result
            }));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ProductFeedbackCreateDto dto)
        {
            try
            {
                var result = await _productFeedbackService.CreateAsync(dto);
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Tạo phản hồi sản phẩm thành công!",
                    data = result
                }));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = ex.Message,
                    data = new List<object>()
                }));
            }
        }

        [HttpPut("update/{productFeedbackId}")]
        public async Task<IActionResult> Update(int productFeedbackId, [FromBody] ProductFeedbackUpdateDto dto)
        {
            var result = await _productFeedbackService.UpdateAsync(productFeedbackId, dto);
            if (result == null)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không tìm thấy phản hồi sản phẩm để cập nhật!",
                    data = new List<object>()
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Cập nhật phản hồi sản phẩm thành công!",
                data = result
            }));
        }

        [HttpDelete("delete/{productFeedbackId}")]
        public async Task<IActionResult> Delete(int productFeedbackId)
        {
            var success = await _productFeedbackService.DeleteAsync(productFeedbackId);
            if (!success)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không tìm thấy phản hồi sản phẩm để xóa!",
                    data = new List<object>()
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Xóa phản hồi sản phẩm thành công!",
                data = new List<object>()
            }));
        }


    }
}
