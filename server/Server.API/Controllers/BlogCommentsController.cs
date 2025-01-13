using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogCommentsController : ControllerBase
    {
        private readonly BlogCommentService _commentService;
        public BlogCommentsController(BlogCommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("check-comment-gross")]
        public async Task<IActionResult> CheckCommentGross(string comment)
        {
            if (string.IsNullOrEmpty(comment))
                return BadRequest();
            var result = await _commentService.CheckCommentGross(comment);
            return Ok(ApiResponse.Succeed(result));
        }
    }
}
