using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System.Linq.Expressions;

namespace Server.Business.Services
{
    public class CategoryService
    {

        private readonly AppDbContext _context;
        private readonly UnitOfWorks unitOfWorks;
        private readonly IMapper _mapper;
        public CategoryService(AppDbContext context, UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _context = context;
            this.unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<Pagination<Category>> GetListAsync(Expression<Func<Category, bool>> filter = null,
                                    Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null,
                                    int? pageIndex = null, // Optional parameter for pagination (page number)
                                    int? pageSize = null)
        {
            IQueryable<Category> query = _context.Categorys;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var totalItemsCount = await query.CountAsync();

            if (pageIndex.HasValue && pageIndex.Value == -1)
            {
                pageSize = totalItemsCount; // Set pageSize to total count
                pageIndex = 0; // Reset pageIndex to 0
            }
            else if (pageIndex.HasValue && pageSize.HasValue)
            {
                int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value : 0;
                int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10; // Assuming a default pageSize of 10 if an invalid value is passed

                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }

            var items = await query.ToListAsync();

            return new Pagination<Category>
            {
                TotalItemsCount = totalItemsCount,
                PageSize = pageSize ?? totalItemsCount,
                PageIndex = pageIndex ?? 0,
                Data = items
            };
        }

        public async Task<ApiResult<Category>> CreateCategoryAsync(CategoryCreateDto categoryCreateDto)
        {
            try
            {
                if (categoryCreateDto == null)
                {
                    return ApiResult<Category>.Error(null);
                }

                if (string.IsNullOrEmpty(categoryCreateDto.Name) || string.IsNullOrEmpty(categoryCreateDto.Description) ||
                    string.IsNullOrEmpty(categoryCreateDto.SkinTypeSuitable) || string.IsNullOrEmpty(categoryCreateDto.ImageUrl))
                {
                    return ApiResult<Category>.Error(null);
                }

                // Tạo danh mục mới
                var newCategory = new Category
                {
                    Name = categoryCreateDto.Name,
                    Description = categoryCreateDto.Description,
                    SkinTypeSuitable = categoryCreateDto.SkinTypeSuitable,
                    Status = "Active",
                    ImageUrl = categoryCreateDto.ImageUrl,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                // Thêm danh mục mới vào cơ sở dữ liệu
                await _context.Categorys.AddAsync(newCategory);
                await _context.SaveChangesAsync();

                // Trả về kết quả thành công với danh mục vừa tạo
                return ApiResult<Category>.Succeed(newCategory);
            }
            catch (Exception ex)
            {
                // Trả về lỗi với ngoại lệ
                return new ApiResult<Category>
                {
                    Success = false,
                    Result = null,
                    ErrorMessage = ex.Message
                };
            }
        }



        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            try
            {

                var category = await _context.Categorys
            .Include(c => c.Products) // Bao gồm danh sách sản phẩm nếu cần
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                return category;
            }
            catch (Exception ex)
            {
                // Xử lý lỗi (nếu có)
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null;  // Trả về null nếu không tìm thấy sản phẩm hoặc có lỗi
            }
        }

        public async Task<GetAllCategoryPaginationResponse> GetAllCategory(int page)
        {
            try
            {
                const int pageSize = 4;

                var categories = await unitOfWorks.CategoryRepository.GetAll()
                    .ToListAsync();

                var totalCount = categories.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var pagedCategories = categories.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                var categoryModels = _mapper.Map<List<CategoryModel>>(pagedCategories);
                return new GetAllCategoryPaginationResponse
                {
                    data = categoryModels,
                    pagination = new Pagination
                    {
                        page = page,
                        totalPage = totalPages,
                        totalCount = totalCount
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving products", ex);
            }
        }

        //public async Task<ApiResult<Category>> UpdateCategoryAsync(int categoryId, CategoryUpdateDto categoryUpdateDto)
        //{
        //    try
        //    {
        //        if (categoryUpdateDto == null)
        //        {
        //            return ApiResult<Category>.Error(null);
        //        }

        //        if (string.IsNullOrEmpty(categoryUpdateDto.Name) || string.IsNullOrEmpty(categoryUpdateDto.Description) ||
        //            string.IsNullOrEmpty(categoryUpdateDto.SkinTypeSuitable) || string.IsNullOrEmpty(categoryUpdateDto.ImageUrl) ||
        //            string.IsNullOrEmpty(categoryUpdateDto.Status))
        //        {
        //            return ApiResult<Category>.Error(new Category { Description = "Invalid value input" });
        //        }

        //        // Kiểm tra giá trị của Status
        //        if (categoryUpdateDto.Status != "Active" && categoryUpdateDto.Status != "Inactive")
        //        {
        //            return ApiResult<Category>.Error(new Category { Description = "Status must be either 'Active' or 'Inactive'" });
        //        }

        //        var existingCategory = await _context.Categorys.FirstOrDefaultAsync(p => p.CategoryId == categoryId);
        //        if (existingCategory == null)
        //        {
        //            return ApiResult<Category>.Error(new Category { Description = "Category not found" });
        //        }

        //        // Cập nhật thông tin
        //        existingCategory.Name = categoryUpdateDto.Name;
        //        existingCategory.Description = categoryUpdateDto.Description;
        //        existingCategory.SkinTypeSuitable = categoryUpdateDto.SkinTypeSuitable;
        //        existingCategory.Status = categoryUpdateDto.Status;
        //        existingCategory.ImageUrl = categoryUpdateDto.ImageUrl;
        //        existingCategory.UpdatedDate = DateTime.Now;

        //        // Lưu các thay đổi vào cơ sở dữ liệu
        //        _context.Categorys.Update(existingCategory);
        //        await _context.SaveChangesAsync();

        //        // Trả về kết quả thành công với sản phẩm vừa cập nhật
        //        return ApiResult<Category>.Succeed(existingCategory);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Trả về lỗi nếu có ngoại lệ
        //        return ApiResult<Category>.Error(new Category { Description = $"Error: {ex.Message}" });
        //    }
        //}

        public async Task<ApiResult<Category>> UpdateCategoryAsync(int categoryId, CategoryUpdateDto categoryUpdateDto)
        {
            try
            {
                if (categoryUpdateDto == null)
                {
                    return new ApiResult<Category>
                    {
                        Success = false,
                        Result = null,
                        ErrorMessage = "Category data is required."
                    };
                }

                if (string.IsNullOrEmpty(categoryUpdateDto.Name) || string.IsNullOrEmpty(categoryUpdateDto.Description) ||
                    string.IsNullOrEmpty(categoryUpdateDto.SkinTypeSuitable) || string.IsNullOrEmpty(categoryUpdateDto.ImageUrl) ||
                    string.IsNullOrEmpty(categoryUpdateDto.Status))
                {
                    return new ApiResult<Category>
                    {
                        Success = false,
                        Result = null,
                        ErrorMessage = "Invalid value input"
                    };
                }

                // Kiểm tra giá trị của Status
                if (categoryUpdateDto.Status != "Active" && categoryUpdateDto.Status != "Inactive")
                {
                    return new ApiResult<Category>
                    {
                        Success = false,
                        Result = null,
                        ErrorMessage = "Status must be either 'Active' or 'Inactive'"
                    };
                }

                var existingCategory = await _context.Categorys.FirstOrDefaultAsync(p => p.CategoryId == categoryId);
                if (existingCategory == null)
                {
                    return new ApiResult<Category>
                    {
                        Success = false,
                        Result = null,
                        ErrorMessage = "Category not found"
                    };
                }

                // Cập nhật thông tin
                existingCategory.Name = categoryUpdateDto.Name;
                existingCategory.Description = categoryUpdateDto.Description;
                existingCategory.SkinTypeSuitable = categoryUpdateDto.SkinTypeSuitable;
                existingCategory.Status = categoryUpdateDto.Status;
                existingCategory.ImageUrl = categoryUpdateDto.ImageUrl;
                existingCategory.UpdatedDate = DateTime.Now;

                // Lưu các thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                // Truy vấn lại bản ghi đã cập nhật từ cơ sở dữ liệu để đảm bảo CategoryId chính xác
                var updatedCategory = await _context.Categorys.FindAsync(categoryId);

                // Trả về kết quả thành công với danh mục đã cập nhật
                return ApiResult<Category>.Succeed(updatedCategory);
            }
            catch (Exception ex)
            {
                // Trả về lỗi nếu có ngoại lệ
                return new ApiResult<Category>
                {
                    Success = false,
                    Result = null,
                    ErrorMessage = $"Error: {ex.Message}"
                };
            }
        }


        public async Task<ApiResult<string>> DeleteCategoryAsync(int categoryId)
        {
            // Tìm danh mục trong cơ sở dữ liệu
            var category = await _context.Categorys.FirstOrDefaultAsync(p => p.CategoryId == categoryId);

            if (category == null)
            {
                return new ApiResult<string>
                {
                    Success = false,
                    Result = null,
                    ErrorMessage = "Category not found."
                };
            }

            // Kiểm tra xem có dịch vụ nào liên kết với danh mục không
            var hasLinkedServices = await _context.Services.AnyAsync(s => s.CategoryId == category.CategoryId);

            if (hasLinkedServices)
            {
                return new ApiResult<string>
                {
                    Success = false,
                    Result = null,
                    ErrorMessage = "Cannot delete category as it is linked to a service."
                };
            }

            // Nếu không có dịch vụ nào liên kết, cập nhật trạng thái thành "Inactive"
            category.Status = "Inactive";
            _context.Categorys.Update(category);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return new ApiResult<string>
            {
                Success = true,
                Result = "Category status updated."
            };
        }




    }
}
