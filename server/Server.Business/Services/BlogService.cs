using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.Repositories;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services
{
    public class BlogService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly CloudianryService _cloudianryService;


        public BlogService(UnitOfWorks unitOfWorks, IMapper mapper, CloudianryService cloudianryService)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _cloudianryService = cloudianryService;
        }

        

        public async Task<GetAllBlogResponse> GetAllBlogs(string status = null, int page = 1, int pageSize = 5)
        {
            // Truy vấn cơ bản với điều kiện lọc `status` nếu có
            var query = _unitOfWorks.BlogRepository
                .FindByCondition(x => string.IsNullOrEmpty(status) || x.Status == status)
                .Include(x => x.Author)
                .OrderByDescending(x => x.BlogId);

            // Lấy tổng số lượng blog sau khi lọc
            var totalCount = await query.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Phân trang dữ liệu
            var pagedBlogs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Kiểm tra kết quả
            if (pagedBlogs == null || !pagedBlogs.Any())
            {
                return null;
            }

            // Ánh xạ sang BlogModel
            var blogsModels = _mapper.Map<List<BlogModel>>(pagedBlogs);

            // Trả về kết quả
            return new GetAllBlogResponse()
            {
                data = blogsModels,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPages,
                    totalCount = totalCount
                }
            };
        }







        public async Task<BlogModel> GetBlogsById(int id)
        {
            // Lấy Blog kèm thông tin User (Author)
            var blog = await _unitOfWorks.BlogRepository
                .FindByCondition(x => x.BlogId == id)
                .Include(x => x.Author) // Include thông tin của Author
                .FirstOrDefaultAsync();

            if (blog == null)
            {
                return null;
            }

            // Map dữ liệu Blog sang BlogModel
            var blogModel = _mapper.Map<BlogModel>(blog);

            // Lấy tên của Author (FullName) nếu Author không null
            blogModel.AuthorName = blog.Author?.FullName;

            return blogModel;
        }

       

        public async Task<BlogModel> CreateBlogs(BlogRequest request, IFormFile thumbnailFile)
        {
            var user = await _unitOfWorks.UserRepository
                .FirstOrDefaultAsync(x => x.UserId == request.AuthorId);

            if (user == null)
            {
                throw new BadRequestException("Author not found!");
            }

            string thumbnailUrl = null;
            if (thumbnailFile != null)
            {
                var uploadResult = await _cloudianryService.UploadImageAsync(thumbnailFile);
                if (uploadResult != null)
                {
                    thumbnailUrl = uploadResult.Uri.ToString();
                    Console.WriteLine("Thumbnail URL: " + thumbnailUrl);
                }
            }

            var newBlog = new Blog
            {
                Title = request.Title,
                AuthorId = request.AuthorId,
                Content = request.Content,
                Thumbnail = thumbnailUrl,
                Status = "Pending",
                Note = !string.IsNullOrEmpty(request.Note) ? request.Note : "",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            var blogEntity = await _unitOfWorks.BlogRepository.AddAsync(newBlog);
            var result = await _unitOfWorks.BlogRepository.Commit();

            if (result > 0)
            {
                var blogModel = _mapper.Map<BlogModel>(blogEntity);
                blogModel.AuthorName = user.FullName;
                blogModel.Thumbnail = blogEntity.Thumbnail;

                Console.WriteLine($"BlogModel: {blogModel.Thumbnail}");
                return blogModel;
            }

            return null;
        }


       

        public async Task<BlogModel> UpdateBlogs(BlogModel blogsModel, BlogRequest request, IFormFile thumbnailFile)
        {
            // 1. Kiểm tra xem Author (User) có tồn tại không
            var user = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.AuthorId);
            if (user == null)
            {
                throw new BadRequestException("Author not found!");
            }

            // 2. Cập nhật các trường nếu có thay đổi
            if (request.AuthorId != 0 && blogsModel.AuthorId != request.AuthorId)
            {
                blogsModel.AuthorId = request.AuthorId;
            }
            if (!string.IsNullOrEmpty(request.Title) && blogsModel.Title != request.Title)
            {
                blogsModel.Title = request.Title;
            }
            if (!string.IsNullOrEmpty(request.Content) && blogsModel.Content != request.Content)
            {
                blogsModel.Content = request.Content;
            }
            if (!string.IsNullOrEmpty(request.Status) && blogsModel.Status != request.Status)
            {
                blogsModel.Status = request.Status;
            }
            if (!string.IsNullOrEmpty(request.Note) && blogsModel.Note != request.Note)
            {
                blogsModel.Note = request.Note;
            }

            // 3. Xử lý upload thumbnail nếu có file
            if (thumbnailFile != null)
            {
                var uploadResult = await _cloudianryService.UploadImageAsync(thumbnailFile);
                if (uploadResult != null)
                {
                    blogsModel.Thumbnail = uploadResult.Uri.ToString();
                }
            }

            // 4. Cập nhật ngày sửa đổi
            blogsModel.UpdatedDate = DateTime.Now;

            // 5. Lưu thay đổi vào database
            var blogEntity = _unitOfWorks.BlogRepository.Update(_mapper.Map<Blog>(blogsModel));
            var result = await _unitOfWorks.BlogRepository.Commit();

            if (result > 0)
            {
                // 6. Map dữ liệu sang BlogModel và gán AuthorName
                var updatedBlogModel = _mapper.Map<BlogModel>(blogEntity);
                updatedBlogModel.AuthorName = user.FullName;

                return updatedBlogModel;
            }

            return null;
        }





        public async Task<BlogModel> DeleteBlogs(BlogModel blogModel)
        {
            // Lấy thông tin Blog hiện tại từ database (bao gồm Author)
            var existingBlog = await _unitOfWorks.BlogRepository
                .FindByCondition(x => x.BlogId == blogModel.BlogId)
                .Include(x => x.Author) // Include để lấy thông tin Author
                .FirstOrDefaultAsync();

            if (existingBlog == null)
            {
                throw new NotFoundException("Blog not found!");
            }

            // Cập nhật trạng thái Blog thành "Rejected"
            existingBlog.Status = "Rejected";

            // Cập nhật Blog trong cơ sở dữ liệu
            _unitOfWorks.BlogRepository.Update(existingBlog);
            var result = await _unitOfWorks.BlogRepository.Commit();

            if (result > 0)
            {
                // Map dữ liệu Blog sang BlogModel và gán AuthorName
                var updatedBlogModel = _mapper.Map<BlogModel>(existingBlog);
                updatedBlogModel.AuthorName = existingBlog.Author?.FullName; // Lấy tên của Author nếu có

                return updatedBlogModel;
            }

            return null;
        }

    }
}
