using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly BlogService _blogService;
        private readonly IMapper _mapper;
        private readonly IElasticClient _elasticClient;
        private readonly ElasticService<BlogDTO> _elasticService;
        private readonly CloudianryService _cloudianryService;

        public BlogController(BlogService blogService, IMapper mapper, IElasticClient elasticClient, CloudianryService cloudianryService)
        {
            _blogService = blogService;
            _mapper = mapper;
            _elasticClient = elasticClient;
            _elasticService = new ElasticService<BlogDTO>(_elasticClient, "blogs");
            _cloudianryService = cloudianryService;
        }


        //[Authorize]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllBlog(
    [FromQuery] string status,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 5)
        {
            var listBlog = await _blogService.GetAllBlogs(status, page, pageSize);
            if (listBlog == null || listBlog.data == null || !listBlog.data.Any())
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Hiện tại không có blog nào!"
                }));
            }
            return Ok(ApiResult<GetAllBlogResponse>.Succeed(new GetAllBlogResponse()
            {
                message = "Lấy danh sách blog thành công!",
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
                    message = "Không tìm thấy blog!"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy blogs thành công!",
                data = _mapper.Map<BlogDTO>(blogsModel)
            }));
        }


        [HttpPost("upload-thumbnail")]
        public async Task<IActionResult> UploadThumbnail(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Không có tệp nào được cung cấp");

                var uploadResult = await _cloudianryService.UploadImageAsync(file);

                if (uploadResult != null)
                {
                    return Ok(ApiResponse.Succeed(new
                    {
                        Url = uploadResult.Uri.ToString(),
                        PublicId = uploadResult.PublicId
                    }, "Tải hình lên thành công."));
                }

                return BadRequest(ApiResponse.Error("Tải lên không thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error("Lỗi máy chủ nội bộ"));
            }
        }


        [HttpGet("elasticsearch")]
        public async Task<IActionResult> ElasticSearch(string? keyword)
        {
            var list = new List<BlogDTO>();
            if (!string.IsNullOrEmpty(keyword))
            {
                list = (await _elasticService.SearchAsync(keyword)).ToList();
            }
            else
                list = (await _elasticService.GetAllAsync()).ToList();
            return Ok(ApiResponse.Succeed(list));
        }

        [HttpPost("create-elastic")]
        public async Task<IActionResult> CreateElastic(BlogDTO model)
        {
            try
            {
                await _elasticService.IndexDocumentAsync(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(model);
        }

        [HttpPost("import-elastic")]
        public async Task<IActionResult> ImportElasticAsync(IFormFile file)
        {
            try
            {
                var result = await _elasticService.ImportFromJsonFileAsync(file);
                return Ok(ApiResponse.Succeed(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpPost("create")]
        public async Task<IActionResult> CreateBlog([FromForm] BlogRequest request)
        {
            // 1. Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            // 2. Gọi service để tạo blog
            var blogsModel = await _blogService.CreateBlogs(request);
            if (blogsModel == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi khi tạo blog!"
                }));
            }

            // 3. Trả về kết quả thành công
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Blog đã được tạo thành công!",
                data = _mapper.Map<BlogDTO>(blogsModel)
            }));
        }




        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateBlog([FromRoute] int id, [FromForm] BlogRequest request)
        {
            // 1. Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            // 2. Kiểm tra blog có tồn tại hay không
            var existingBlog = await _blogService.GetBlogsById(id);
            if (existingBlog == null)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy blog!"
                }));
            }

            // 3. Gọi service để cập nhật blog
            var updatedBlog = await _blogService.UpdateBlogs(existingBlog, request);
            if (updatedBlog == null)
            {
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Đã xảy ra lỗi khi cập nhật blog!"
                }));
            }

            // 4. Trả về kết quả thành công
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Blog đã được cập nhật thành công!",
                data = _mapper.Map<BlogDTO>(updatedBlog)
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
                    message = "Không tìm thấy blog!"
                }));
            }

            var blogsModel = await _blogService.DeleteBlogs(blogsExist);
            if (blogsModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Có lỗi khi xóa blog!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Xóa blog thành công!",
                data = _mapper.Map<BlogDTO>(blogsModel)
            }));
        }
    }
}
