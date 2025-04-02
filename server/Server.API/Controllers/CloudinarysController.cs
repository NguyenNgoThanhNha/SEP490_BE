using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CloudinarysController : ControllerBase
    {
        private readonly CloudianryService _cloudinaryService;
        public CloudinarysController(CloudianryService cloudianryService)
        {
            _cloudinaryService = cloudianryService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Không có tập tin");

                var uploadResult = await _cloudinaryService.UploadImageAsync(file);

                if (uploadResult != null)
                {
                    return Ok(ApiResponse.Succeed(new
                    {
                        Url = uploadResult.Uri.ToString(),
                        PublicId = uploadResult.PublicId
                    }, "Tải ảnh lên thành công."));
                }

                return BadRequest(ApiResponse.Error("Tải lên không thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error("Lỗi máy chủ nội bộ"));
            }
        }
    }
}
