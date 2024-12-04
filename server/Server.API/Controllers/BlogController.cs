using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Services;
using Server.Business.Dtos;
using Server.Business.Commons.Request;
using Server.Business.Ultils;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly BlogService _blogService;
        private readonly IMapper _mapper;

        public BlogController(BlogService blogService, IMapper mapper)
        {
            _blogService = blogService;
            _mapper = mapper;           
        }

       // [Authorize]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllBlog([FromQuery] int page = 1, int pageSize = 5)
        {
            var listBlog = await _blogService.GetAllBlogs(page, pageSize);
            if (listBlog.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Currently, there is no blogs!"
                }));
            }
            return Ok(ApiResult<GetAllBlogResponse>.Succeed(new GetAllBlogResponse()
            {
                message = "Get blogs successfully!",
                data = listBlog.data,
                pagination = listBlog.pagination
            }));
        }

       // [Authorize]
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetByBlogId([FromRoute] int id)
        {
            var blogsModel = await _blogService.GetBlogsById(id);
            if (blogsModel == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Blog not found!"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Get blogs successfully!",
                data = _mapper.Map<BlogDTO>(blogsModel)
            }));
        }

        //[Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateBlog([FromBody] BlogRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var blogsModel = await _blogService.CreateBlogs(request);
            if (blogsModel == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Error in create blogs!"
                }));
            }    

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Blog created successfully!",
                data = _mapper.Map<BlogDTO>(blogsModel)
            }));
        }

        //[Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateBlog([FromRoute] int id, [FromBody] BlogRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var blogsExist = await _blogService.GetBlogsById(id);
            if (blogsExist.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Blog not found!"
                }));
            }

            var blogsModel = await _blogService.UpdateBlogs(blogsExist, request);
            if (blogsExist.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Error in update blogs!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Update blogs successfully!",
                data = _mapper.Map<BlogDTO>(blogsModel)
            }));
        }

        //[Authorize]
        [HttpPut("delete/{id}")]
        public async Task<IActionResult> DeleteBlog([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var blogsExist = await _blogService.GetBlogsById(id);
            if (blogsExist.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Blog not found!"
                }));
            }

            var blogsModel = await _blogService.DeleteBlogs(blogsExist);
            if (blogsModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Error in delete blogs!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Delete blogs successfully!",
                data = _mapper.Map<BlogDTO>(blogsModel)
            }));
        }
    }
}
