using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Services;

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
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
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

    }
}
