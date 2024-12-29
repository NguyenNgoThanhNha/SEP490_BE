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
    public class ProductService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        public ProductService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }
        public async Task<Pagination<ProductDto>> GetListAsync(
    Expression<Func<Product, bool>> filter = null,
    Func<IQueryable<Product>, IOrderedQueryable<Product>> orderBy = null,
    string includeProperties = "",
    int? pageIndex = null,
    int? pageSize = null)
        {
            try
            {
                // Truy vấn với Include đầy đủ thông tin Category và Company
                IQueryable<Product> query = _unitOfWorks.ProductRepository.GetAll()
                    .Include(p => p.Category)
                    .Include(p => p.Company)
                 .OrderByDescending(p => p.ProductId);

                // Include các bảng bổ sung từ tham số
                foreach (var includeProperty in includeProperties.Split(
                             new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }

                // Áp dụng bộ lọc nếu có
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                // Áp dụng sắp xếp nếu có
                if (orderBy != null)
                {
                    query = orderBy(query);
                }
                else
                {
                    query = query.OrderByDescending(p => p.ProductId); // Mặc định sắp xếp theo ProductId tăng dần
                }

                // Tổng số lượng sản phẩm
                var totalItemsCount = await query.CountAsync();

                // Phân trang
                if (pageIndex.HasValue && pageSize.HasValue)
                {
                    query = query.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value);
                }

                // Lấy dữ liệu
                var items = await query.ToListAsync();

                // Lấy hình ảnh cho tất cả sản phẩm một lần
                var productIds = items.Select(p => p.ProductId).ToList();
                var productImages = await _unitOfWorks.ProductImageRepository.GetAll()
                    .Where(si => productIds.Contains(si.ProductId))
                    .GroupBy(si => si.ProductId)
                    .ToDictionaryAsync(g => g.Key, g => g.Select(si => si.image).ToArray());

                // Mapping DTO
                var data = items.Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    Price = p.Price,
                    Volume = p.Volume,
                    Dimension = p.Dimension,
                    Quantity = p.Quantity,
                    Discount = p.Discount,
                    Status = p.Status,
                    CompanyId = p.Company != null ? p.Company.CompanyId : 0, // Kiểm tra null
                    CompanyName = p.Company != null ? p.Company.Name : string.Empty, // Kiểm tra null
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,
                    CategoryId = p.CategoryId,
                    Category = p.Category != null ? new CategoryDto
                    {
                        CategoryId = p.Category.CategoryId,
                        Name = p.Category.Name,
                        Description = p.Category.Description,
                        SkinTypeSuitable = p.Category.SkinTypeSuitable,
                        Status = p.Category.Status,
                        ImageUrl = p.Category.ImageUrl,
                        CreatedDate = p.Category.CreatedDate,
                        UpdatedDate = p.Category.UpdatedDate
                    } : null, // Nếu Category null, gán null
                    images = productImages.ContainsKey(p.ProductId)
                        ? productImages[p.ProductId]
                        : Array.Empty<string>() // Nếu không có hình ảnh, gán mảng rỗng
                }).ToList();

                return new Pagination<ProductDto>
                {
                    TotalItemsCount = totalItemsCount,
                    PageSize = pageSize ?? totalItemsCount,
                    PageIndex = pageIndex ?? 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving product list", ex);
            }
        }


        public async Task<Branch_Product?> GetProductInBranchAsync(int productId, int branchId)
        {
            // Sử dụng UnitOfWorks để truy cập dữ liệu
            var productInBranch = await _unitOfWorks.Brand_ProductRepository
                .FindByCondition(bp => bp.ProductId == productId &&
                                       bp.BranchId == branchId &&
                                       bp.Status == "Active")
                .Include(bp => bp.Product) // Bao gồm thông tin sản phẩm
                .FirstOrDefaultAsync();

            // Trả về sản phẩm hoặc null nếu không tìm thấy
            return productInBranch;
        }


        public async Task<ApiResult<ApiResponse>> CreateProductAsync(ProductCreateDto productCreateDto)
        {
            try
            {
                if (productCreateDto == null)
                {
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Please enter complete information."
                    });
                }

                // Kiểm tra Category có tồn tại không
                var categoryExists = await _unitOfWorks.CategoryRepository
     .FindByCondition(c => c.CategoryId == productCreateDto.CategoryId)
     .AnyAsync();


                if (!categoryExists)
                {
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Category does not exist."
                    });
                }

                // Kiểm tra Company có tồn tại không
                var companyExists = await _unitOfWorks.CompanyRepository
    .FindByCondition(c => c.CompanyId == productCreateDto.CompanyId)
    .AnyAsync();

                if (!companyExists)
                {
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Company does not exist."
                    });
                }

                // Tạo sản phẩm mới
                var newProduct = new Product
                {
                    ProductName = productCreateDto.ProductName,
                    ProductDescription = productCreateDto.ProductDescription,
                    Price = productCreateDto.Price,
                    Quantity = productCreateDto.Quantity,
                    Discount = productCreateDto.Discount,
                    CategoryId = productCreateDto.CategoryId,
                    CompanyId = productCreateDto.CompanyId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Volume = productCreateDto.Volume,
                    Dimension = productCreateDto.Dimension,
                    Status = "Active"
                };

                await _unitOfWorks.ProductRepository.AddAsync(newProduct);
                await _unitOfWorks.ProductRepository.Commit();


                // Lấy lại sản phẩm vừa tạo từ cơ sở dữ liệu kèm theo thông tin của Category và Company
                var createdProduct = await _unitOfWorks.ProductRepository
     .FindByCondition(p => p.ProductId == newProduct.ProductId)
     .Include(p => p.Category)
     .Include(p => p.Company)
     .FirstOrDefaultAsync();


                if (createdProduct == null)
                {
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Failed to retrieve the created product."
                    });
                }

                return ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Product created successfully.",
                    data = new
                    {
                        createdProduct.ProductId,
                        createdProduct.ProductName,
                        createdProduct.ProductDescription,
                        createdProduct.Price,
                        createdProduct.Quantity,
                        createdProduct.Discount,
                        createdProduct.Status,
                        createdProduct.CategoryId,
                        CategoryName = createdProduct.Category?.Name,
                        createdProduct.CompanyId,
                        CompanyName = createdProduct.Company?.Name,
                        createdProduct.Volume,
                        createdProduct.Dimension,
                        createdProduct.CreatedDate,
                        createdProduct.UpdatedDate
                    }
                });
            }
            catch (Exception ex)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Error: {ex.Message}"
                });
            }
        }

        public async Task<ProductModel?> GetProductByIdAsync(int productId)
        {
            try
            {
                // Lấy sản phẩm có trạng thái "Active"
                var product = await _unitOfWorks.ProductRepository
                    .FindByCondition(p => p.ProductId == productId && p.Status == "Active")
                    .Include(d => d.Category)
                    .Include(d => d.Company)
                    .FirstOrDefaultAsync();

                // Kiểm tra nếu không tìm thấy sản phâm
                if (product == null)
                    return null;

                var productModel = _mapper.Map<ProductModel>(product);
            
                var serviceImages = await _unitOfWorks.ProductImageRepository.FindByCondition(x => x.ProductId == productModel.ProductId)
                    .Where(si => si.ProductId == product.ProductId)
                    .Select(si => si.image)
                    .ToArrayAsync();

                productModel.images = serviceImages;
                return productModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null;
            }
        }

     
        public async Task<GetAllProductPaginationResponse> GetAllProduct(int page, int pageSize)
        {
            // Lấy dữ liệu sản phẩm với Category và áp dụng phân trang trực tiếp trên IQueryable
            var query = _unitOfWorks.ProductRepository.GetAll()
                .Include(p => p.Category) // Bao gồm thông tin Category
                .Include(p => p.Company)
                 .OrderByDescending(p => p.ProductId);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Ánh xạ sang ProductModel
            var productModels = _mapper.Map<List<ProductModel>>(products);

            // Lấy hình ảnh cho tất cả sản phẩm một lần
            var productIds = products.Select(p => p.ProductId).ToList();
            var productImages = await _unitOfWorks.ProductImageRepository.GetAll()
                .Where(si => productIds.Contains(si.ProductId))
                .GroupBy(si => si.ProductId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(si => si.image).ToArray());

            // Gán hình ảnh và category cho từng sản phẩm
            foreach (var product in productModels)
            {
                product.images = productImages.ContainsKey(product.ProductId)
                    ? productImages[product.ProductId]
                    : Array.Empty<string>();

             
            }

            return new GetAllProductPaginationResponse
            {
                data = productModels,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPages,
                    totalCount = totalCount
                }
            };
        }




        public async Task<ApiResult<ApiResponse>> UpdateProductAsync(int productId, ProductUpdateDto productUpdateDto)
        {
            try
            {
                if (productUpdateDto == null)
                {
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Product update data is required."
                    });
                }

                if (productUpdateDto.Price <= 0 || productUpdateDto.Quantity <= 0 || productUpdateDto.Discount < 0)
                {
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Invalid Price, Quantity, or Discount."
                    });
                }

                // Kiểm tra sự tồn tại của Product
                var existingProduct = await _unitOfWorks.ProductRepository
                    .FindByCondition(p => p.ProductId == productId)
                    .FirstOrDefaultAsync();


                if (existingProduct == null)
                {
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Product not found."
                    });
                }

                // Kiểm tra sự tồn tại của Category và Company
                var categoryExists = await _unitOfWorks.CategoryRepository
     .FindByCondition(c => c.CategoryId == productUpdateDto.CategoryId)
     .AnyAsync();


                var companyExists = await _unitOfWorks.CompanyRepository
      .FindByCondition(c => c.CompanyId == productUpdateDto.CompanyId)
      .AnyAsync();


                if (!categoryExists)
                {
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Category does not exist."
                    });
                }

                if (!companyExists)
                {
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Company does not exist."
                    });
                }

                // Cập nhật thông tin Product
                existingProduct.ProductName = productUpdateDto.ProductName;
                existingProduct.ProductDescription = productUpdateDto.ProductDescription;
                existingProduct.Price = productUpdateDto.Price;
                existingProduct.Quantity = productUpdateDto.Quantity;
                existingProduct.Discount = productUpdateDto.Discount;
                existingProduct.CategoryId = productUpdateDto.CategoryId;
                existingProduct.CompanyId = productUpdateDto.CompanyId;
                existingProduct.UpdatedDate = DateTime.Now;

                // Lưu thay đổi
                _unitOfWorks.ProductRepository.Update(existingProduct);
                await _unitOfWorks.ProductRepository.Commit();


                // Lấy lại Product sau khi cập nhật kèm theo Category và Company
                var updatedProduct = await _unitOfWorks.ProductRepository
    .FindByCondition(p => p.ProductId == productId)
    .Include(p => p.Category)
    .Include(p => p.Company)
    .FirstOrDefaultAsync();


                return ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Product updated successfully.",
                    data = new
                    {
                        updatedProduct.ProductId,
                        updatedProduct.ProductName,
                        updatedProduct.ProductDescription,
                        updatedProduct.Price,
                        updatedProduct.Quantity,
                        updatedProduct.Discount,
                        updatedProduct.Status,
                        updatedProduct.CategoryId,
                        CategoryName = updatedProduct.Category?.Name,
                        updatedProduct.CompanyId,
                        CompanyName = updatedProduct.Company?.Name,
                        updatedProduct.CreatedDate,
                        updatedProduct.UpdatedDate
                    }
                });
            }
            catch (Exception ex)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Error: {ex.Message}"
                });
            }
        }

        public async Task<ApiResponse> DeleteProductAsync(int productId)
        {
            try
            {
                // Tìm sản phẩm theo ProductId
                var product = await _unitOfWorks.ProductRepository
    .FindByCondition(p => p.ProductId == productId)
    .Include(p => p.Branch_Products) // Bao gồm danh sách sản phẩm chi nhánh liên kết
    .FirstOrDefaultAsync();


                // Kiểm tra nếu sản phẩm không tồn tại
                if (product == null)
                {
                    return ApiResponse.Error("Product not found.");
                }

                // Kiểm tra xem danh mục của sản phẩm có liên kết với bất kỳ dịch vụ nào không
                /*var hasLinkedServices = await _unitOfWorks.ServiceRepository
                    .FindByCondition(s => s.CategoryId == product.CategoryId)
                    .AnyAsync();

                if (hasLinkedServices)
                {
                    return ApiResponse.Error("Cannot delete product as its category is linked to a service.");
                }*/

                // Cập nhật trạng thái sản phẩm thành "Inactive"
                product.Status = "Inactive";

                // Cập nhật sản phẩm thông qua UnitOfWork
                _unitOfWorks.ProductRepository.Update(product);
               


                // Lưu thay đổi vào cơ sở dữ liệu
                var result = await _unitOfWorks.ProductRepository.Commit();

                if (result > 0)
                {
                    return ApiResponse.Succeed("Product status updated to 'Inactive' successfully.");
                }

                return ApiResponse.Error("Failed to update product status.");
            }
            catch (Exception ex)
            {
                // Trả về lỗi nếu có ngoại lệ
                return ApiResponse.Error($"Error: {ex.Message}");
            }
        }

    }
}



