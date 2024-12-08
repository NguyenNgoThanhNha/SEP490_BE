using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services
{
    public class BlogService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;


        public BlogService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<GetAllBlogResponse> GetAllBlogs(int page = 1, int pageSize = 5)
        {
            var listBlogs = await _unitOfWorks.BlogRepository.FindByCondition(x => x.Status == "Accept").Include(x => x.Author).OrderByDescending(x => x.BlogId).ToListAsync();
            if (listBlogs.Equals(null))
            {
                return null;
            }
            var totalCount = listBlogs.Count();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedServices = listBlogs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var blogsModels = _mapper.Map<List<BlogModel>>(pagedServices);



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

        public async Task<BlogModel> CreateBlogs(BlogRequest request)
        {
            // Kiểm tra xem Author có tồn tại không
            var user = await _unitOfWorks.UserRepository
                .FirstOrDefaultAsync(x => x.UserId == request.AuthorId);

            if (user == null)
            {
                throw new BadRequestException("Author not found!");
            }

            // Tạo thực thể Blog mới từ request
            var newBlog = new Blog
            {
                Title = request.Title,
                AuthorId = request.AuthorId,
                Content = request.Content,
                Status = "Pending",
                Note = !string.IsNullOrEmpty(request.Note) ? request.Note : "",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            // Thêm blog vào cơ sở dữ liệu
            var blogEntity = await _unitOfWorks.BlogRepository.AddAsync(newBlog);

            // Lưu thay đổi vào database
            var result = await _unitOfWorks.BlogRepository.Commit();
            if (result > 0)
            {
                // Map dữ liệu sang BlogModel và gán AuthorName
                var blogModel = _mapper.Map<BlogModel>(blogEntity);
                blogModel.AuthorName = user.FullName; // Gán tên của Author từ user

                return blogModel;
            }

            return null;
        }


        public async Task<BlogModel> UpdateBlogs(BlogModel blogsModel, BlogRequest request)
        {
            // Kiểm tra xem Author (User) có tồn tại không
            var user = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.AuthorId);
            if (user == null)
            {
                throw new BadRequestException("Author not found!");
            }

            // Cập nhật các trường nếu có thay đổi trong request
            if (request.AuthorId != null)
            {
                blogsModel.AuthorId = request.AuthorId;
            }
            if (!string.IsNullOrEmpty(request.Title))
            {
                blogsModel.Title = request.Title;
            }
            if (!string.IsNullOrEmpty(request.Content))
            {
                blogsModel.Content = request.Content;
            }
            if (!string.IsNullOrEmpty(request.Status))
            {
                blogsModel.Status = request.Status;
            }
            if (!string.IsNullOrEmpty(request.Note))
            {
                blogsModel.Note = request.Note;
            }

            // Cập nhật ngày sửa đổi
            blogsModel.UpdatedDate = DateTime.Now;

            // Cập nhật dữ liệu blog trong database
            var blogsEntity = _unitOfWorks.BlogRepository.Update(_mapper.Map<Blog>(blogsModel));
            var result = await _unitOfWorks.BlogRepository.Commit();

            if (result > 0)
            {
                // Map dữ liệu sang BlogModel và gán AuthorName
                var updatedBlogModel = _mapper.Map<BlogModel>(blogsEntity);
                updatedBlogModel.AuthorName = user.FullName; // Gán tên của Author

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
