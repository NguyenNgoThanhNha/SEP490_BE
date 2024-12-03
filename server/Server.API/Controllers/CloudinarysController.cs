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
                    return BadRequest("Not file");

                var uploadResult = await _cloudinaryService.UploadImageAsync(file);

                if (uploadResult != null)
                {
                    return Ok(ApiResponse.Succeed(new
                    {
                        Url = uploadResult.Uri.ToString(),
                        PublicId = uploadResult.PublicId
                    }, "Upload image successfully."));
                }

                return BadRequest(ApiResponse.Error("Upload fail"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error("Internal server error"));
            }
        }
    }
}
