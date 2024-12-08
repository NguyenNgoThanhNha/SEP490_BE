﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System.Linq.Expressions;

namespace Server.Business.Services
{
    public class CategoryService
    {

        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        public CategoryService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<Pagination<CategoryDto>> GetListAsync(
    Expression<Func<Category, bool>> filter = null,
    Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null,
    int? pageIndex = 0,
    int? pageSize = 10)
        {
            IQueryable<Category> query = _unitOfWorks.CategoryRepository.GetAll()
      .Include(c => c.Products) // Bao gồm danh sách sản phẩm
          .ThenInclude(p => p.Company); // Bao gồm thông tin công ty của sản phẩm


            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var totalItemsCount = await query.CountAsync();

            if (totalItemsCount == 0)
            {
                return new Pagination<CategoryDto>
                {
                    TotalItemsCount = 0,
                    PageSize = pageSize ?? 10,
                    PageIndex = pageIndex ?? 0,
                    Data = new List<CategoryDto>()
                };
            }

            pageIndex = pageIndex.HasValue && pageIndex.Value >= 0 ? pageIndex.Value : 0;
            pageSize = pageSize.HasValue && pageSize.Value > 0 ? pageSize.Value : 10;

            query = query.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value);

            var categories = await query.ToListAsync();

            var categoryDtos = categories.Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                SkinTypeSuitable = c.SkinTypeSuitable,
                Status = c.Status,
                ImageUrl = c.ImageUrl,
                CreatedDate = c.CreatedDate,
                UpdatedDate = c.UpdatedDate,
                Products = c.Products?.Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Discount = p.Discount,
                    Status = p.Status,
                    CategoryId = p.CategoryId,
                    CompanyId = p.CompanyId,
                    CompanyName = p.Company.Name,
                    CategoryName = p.Category.Name,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate
                }).ToList() ?? new List<ProductDto>()
            }).ToList();

            var totalPagesCount = (int)Math.Ceiling(totalItemsCount / (double)pageSize.Value);

            return new Pagination<CategoryDto>
            {
                TotalItemsCount = totalItemsCount,
                PageSize = pageSize.Value,
                PageIndex = pageIndex.Value,
                Data = categoryDtos
            };
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto categoryCreateDto)
        {
            // Kiểm tra danh mục đã tồn tại hay chưa
            var existingCategory = await _unitOfWorks.CategoryRepository
                .FindByCondition(c => c.Name.ToLower() == categoryCreateDto.Name.ToLower())
                .FirstOrDefaultAsync();

            if (existingCategory != null)
            {
                return null; // Indicate failure due to duplicate
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

            // Thêm vào cơ sở dữ liệu
            await _unitOfWorks.CategoryRepository.AddAsync(newCategory);

            // Lưu thay đổi
            var result = await _unitOfWorks.CategoryRepository.Commit();

            if (result > 0)
            {
                // Map entity sang DTO
                var categoryDto = new CategoryDto
                {
                    CategoryId = newCategory.CategoryId,
                    Name = newCategory.Name,
                    Description = newCategory.Description,
                    SkinTypeSuitable = newCategory.SkinTypeSuitable,
                    Status = newCategory.Status,
                    ImageUrl = newCategory.ImageUrl,
                    CreatedDate = newCategory.CreatedDate,
                    UpdatedDate = newCategory.UpdatedDate,
                    Products = new List<ProductDto>() // Mặc định chưa có sản phẩm
                };

                return categoryDto;
            }

            return null; // Indicate failure due to database issues
        }

        public async Task<CategoryModel> GetCategoryByIdAsync(int categoryId)
        {
            // Lấy danh mục từ repository
            var category = await _unitOfWorks.CategoryRepository
    .FindByCondition(c => c.CategoryId == categoryId)
    .Include(c => c.Products) // Bao gồm sản phẩm
    .ThenInclude(p => p.Company) // Bao gồm thông tin công ty
    .FirstOrDefaultAsync();

            // Kiểm tra nếu không tìm thấy danh mục
            if (category == null)
            {
                throw new NotFoundException("Category not found.");
            }

            // Map dữ liệu sang CategoryModel (hoặc DTO)
            var categoryModel = new CategoryModel
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                SkinTypeSuitable = category.SkinTypeSuitable,
                Status = category.Status,
                ImageUrl = category.ImageUrl,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate,
                Products = category.Products?.Select(p => new ProductModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Discount = p.Discount,
                    Status = p.Status,
                    CategoryId = p.CategoryId,
                    CompanyId = p.CompanyId,
                    CompanyName = p.Company.Name,
                    CategoryName = p.Category.Name,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate
                }).ToList()
            };

            return categoryModel;
        }

        public async Task<GetAllCategoryPaginationResponse> GetAllCategoryAsync(int page = 1, int pageSize = 4)
        {
            try
            {
                // Lấy danh sách tất cả các danh mục từ repository
                var categoriesQuery = _unitOfWorks.CategoryRepository.GetAll()
                    .Include(c => c.Products) // Bao gồm danh sách sản phẩm nếu cần
                    .ThenInclude(p => p.Company); // Bao gồm thông tin công ty của sản phẩm

                // Lấy danh sách danh mục
                var categories = await categoriesQuery.OrderBy(c => c.CategoryId).ToListAsync();

                if (categories == null || categories.Count == 0)
                {
                    return new GetAllCategoryPaginationResponse
                    {
                        data = new List<CategoryModel>(),
                        pagination = new Pagination
                        {
                            page = page,
                            totalPage = 0,
                            totalCount = 0
                        }
                    };
                }

                // Tổng số danh mục
                var totalCount = categories.Count;

                // Tính tổng số trang
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Phân trang
                var pagedCategories = categories.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // Map từ entity sang model
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
                throw new Exception($"Error retrieving categories: {ex.Message}", ex);
            }
        }

        public async Task<CategoryModel> UpdateCategoryAsync(int categoryId, CategoryUpdateDto categoryUpdateDto)
        {
            // Kiểm tra đầu vào
            if (categoryUpdateDto == null)
            {
                throw new BadRequestException("Category data is required.");
            }

            if (string.IsNullOrEmpty(categoryUpdateDto.Name) ||
                string.IsNullOrEmpty(categoryUpdateDto.Description) ||
                string.IsNullOrEmpty(categoryUpdateDto.SkinTypeSuitable) ||
                string.IsNullOrEmpty(categoryUpdateDto.ImageUrl) ||
                string.IsNullOrEmpty(categoryUpdateDto.Status))
            {
                throw new BadRequestException("Invalid value input.");
            }

            // Kiểm tra giá trị của Status
            if (categoryUpdateDto.Status != "Active" && categoryUpdateDto.Status != "Inactive")
            {
                throw new BadRequestException("Status must be either 'Active' or 'Inactive'.");
            }

            // Lấy danh mục hiện tại từ repository
            var existingCategory = await _unitOfWorks.CategoryRepository.GetByIdAsync(categoryId);
            if (existingCategory == null)
            {
                throw new NotFoundException("Category not found.");
            }

            // Cập nhật thông tin danh mục
            existingCategory.Name = categoryUpdateDto.Name;
            existingCategory.Description = categoryUpdateDto.Description;
            existingCategory.SkinTypeSuitable = categoryUpdateDto.SkinTypeSuitable;
            existingCategory.Status = categoryUpdateDto.Status;
            existingCategory.ImageUrl = categoryUpdateDto.ImageUrl;
            existingCategory.UpdatedDate = DateTime.Now;

            // Cập nhật danh mục thông qua repository
            _unitOfWorks.CategoryRepository.Update(existingCategory);

            // Lưu các thay đổi vào cơ sở dữ liệu
            var result = await _unitOfWorks.CategoryRepository.Commit();
            if (result <= 0)
            {
                throw new Exception("Failed to update category.");
            }

            // Ánh xạ sang model
            var updatedCategoryModel = _mapper.Map<CategoryModel>(existingCategory);
            return updatedCategoryModel;
        }
        public async Task<CategoryModel> DeleteCategoryAsync(int categoryId)
        {
            // Tìm danh mục từ cơ sở dữ liệu thông qua UnitOfWork
            var category = await _unitOfWorks.CategoryRepository.GetByIdAsync(categoryId);

            if (category == null)
            {
                throw new BadRequestException("Category not found!");
            }

            // Kiểm tra xem có dịch vụ nào liên kết với danh mục không
            var hasLinkedServices = await _unitOfWorks.ServiceRepository
                .FindByCondition(s => s.CategoryId == category.CategoryId)
                .AnyAsync();

            if (hasLinkedServices)
            {
                throw new BadRequestException("Cannot delete category as it is linked to a service!");
            }

            // Cập nhật trạng thái thành "Inactive"
            category.Status = "Inactive";
            category.UpdatedDate = DateTime.Now;

            // Cập nhật danh mục trong cơ sở dữ liệu
            _unitOfWorks.CategoryRepository.Update(category);

            // Lưu thay đổi
            var result = await _unitOfWorks.CategoryRepository.Commit();

            if (result <= 0)
            {
                throw new InvalidOperationException("Failed to update category status.");
            }

            // Map dữ liệu danh mục thành CategoryModel và trả về
            return _mapper.Map<CategoryModel>(category);
        }
    }
}