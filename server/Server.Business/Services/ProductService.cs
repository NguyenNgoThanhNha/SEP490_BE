using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nest;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using Service.Business.Services;
using System.Linq.Expressions;

namespace Server.Business.Services
{
    public class ProductService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly CloudianryService _cloudianryService;
        private readonly IAIMLService _gptService;
        public ProductService(UnitOfWorks unitOfWorks, IMapper mapper, CloudianryService cloudianryService, IAIMLService gptService)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _cloudianryService = cloudianryService;
            _gptService = gptService;
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
                    SkinTypeSuitable = p.SkinTypeSuitable,
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
            var productInBranch = await _unitOfWorks.Branch_ProductRepository
                .FindByCondition(bp => bp.ProductId == productId &&
                                       bp.BranchId == branchId &&
                                       bp.Status == "Active")
                .Include(bp => bp.Product) // Bao gồm thông tin sản phẩm
                .FirstOrDefaultAsync();

            // Trả về sản phẩm hoặc null nếu không tìm thấy
            return productInBranch;
        }


        //    public async Task<ApiResult<ApiResponse>> CreateProductAsync(ProductCreateDto productCreateDto)
        //    {
        //        try
        //        {
        //            if (productCreateDto == null)
        //            {
        //                return ApiResult<ApiResponse>.Error(new ApiResponse
        //                {
        //                    message = "Please enter complete information."
        //                });
        //            }

        //            // Kiểm tra Category có tồn tại không
        //            var categoryExists = await _unitOfWorks.CategoryRepository
        // .FindByCondition(c => c.CategoryId == productCreateDto.CategoryId)
        // .AnyAsync();


        //            if (!categoryExists)
        //            {
        //                return ApiResult<ApiResponse>.Error(new ApiResponse
        //                {
        //                    message = "Category does not exist."
        //                });
        //            }

        //            // Kiểm tra Company có tồn tại không
        //            var companyExists = await _unitOfWorks.CompanyRepository
        //.FindByCondition(c => c.CompanyId == productCreateDto.CompanyId)
        //.AnyAsync();

        //            if (!companyExists)
        //            {
        //                return ApiResult<ApiResponse>.Error(new ApiResponse
        //                {
        //                    message = "Company does not exist."
        //                });
        //            }

        //            // Tạo sản phẩm mới
        //            var newProduct = new Product
        //            {
        //                ProductName = productCreateDto.ProductName,
        //                ProductDescription = productCreateDto.ProductDescription,
        //                Price = productCreateDto.Price,
        //                Quantity = productCreateDto.Quantity,
        //                Discount = productCreateDto.Discount,
        //                CategoryId = productCreateDto.CategoryId,
        //                CompanyId = productCreateDto.CompanyId,
        //                CreatedDate = DateTime.Now,
        //                UpdatedDate = DateTime.Now,
        //                Volume = productCreateDto.Volume,
        //                Dimension = productCreateDto.Dimension,
        //                Status = "Active"
        //            };

        //            await _unitOfWorks.ProductRepository.AddAsync(newProduct);
        //            await _unitOfWorks.ProductRepository.Commit();


        //            // Lấy lại sản phẩm vừa tạo từ cơ sở dữ liệu kèm theo thông tin của Category và Company
        //            var createdProduct = await _unitOfWorks.ProductRepository
        // .FindByCondition(p => p.ProductId == newProduct.ProductId)
        // .Include(p => p.Category)
        // .Include(p => p.Company)
        // .FirstOrDefaultAsync();


        //            if (createdProduct == null)
        //            {
        //                return ApiResult<ApiResponse>.Error(new ApiResponse
        //                {
        //                    message = "Failed to retrieve the created product."
        //                });
        //            }

        //            return ApiResult<ApiResponse>.Succeed(new ApiResponse
        //            {
        //                message = "Product created successfully.",
        //                data = new
        //                {
        //                    createdProduct.ProductId,
        //                    createdProduct.ProductName,
        //                    createdProduct.ProductDescription,
        //                    createdProduct.Price,
        //                    createdProduct.Quantity,
        //                    createdProduct.Discount,
        //                    createdProduct.Status,
        //                    createdProduct.CategoryId,
        //                    CategoryName = createdProduct.Category?.Name,
        //                    createdProduct.CompanyId,
        //                    CompanyName = createdProduct.Company?.Name,
        //                    createdProduct.Volume,
        //                    createdProduct.Dimension,
        //                    createdProduct.CreatedDate,
        //                    createdProduct.UpdatedDate
        //                }
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            return ApiResult<ApiResponse>.Error(new ApiResponse
        //            {
        //                message = $"Error: {ex.Message}"
        //            });
        //        }
        //    }

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

                // Upload ảnh nếu có
                string? imageUrl = null;
                if (productCreateDto.Image != null)
                {
                    var imageUploadResult = await _cloudianryService.UploadImageAsync(productCreateDto.Image);
                    if (imageUploadResult == null)
                    {
                        return ApiResult<ApiResponse>.Error(new ApiResponse
                        {
                            message = "Upload image failed!"
                        });
                    }
                    imageUrl = imageUploadResult.SecureUrl.ToString();
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

                // Lưu ảnh vào ProductImages nếu có ảnh
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var productImage = new ProductImages
                    {
                        ProductId = newProduct.ProductId,
                        image = imageUrl
                    };
                    await _unitOfWorks.ProductImageRepository.AddAsync(productImage);
                    await _unitOfWorks.ProductImageRepository.Commit();
                }

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
                        createdProduct.UpdatedDate,
                        ImageUrl = imageUrl
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



        public async Task<List<Product>> GetTop5BestSellersAsync()
        {
            // Lấy danh sách OrderDetail hoàn thành
            var orderDetails = await _unitOfWorks.OrderDetailRepository
     .FindByCondition(od => od.Status == "Completed")
     .Include(od => od.Product)
         .ThenInclude(p => p.Category) // Bao gồm Category
     .Include(od => od.Product)
         .ThenInclude(p => p.Company) // Bao gồm Company
     .ToListAsync();


            // Nhóm và tính toán
            var bestSellers = orderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    Product = g.First().Product, // Lấy toàn bộ object Product
                    QuantitySold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(5)
                .Select(p => p.Product) // Lấy danh sách object Product
                .ToList();

            return bestSellers;
        }

        public async Task<GrossDTO> CheckInputHasGross(string name)
        {
            var result = await _gptService.GetGross(name);
            GrossDTO gross = new GrossDTO()
            {
                Grosses = result,
                HasGross = result != null && result.Count > 0
            };
            return gross;
        }

        public async Task<List<ProductModel>> GetListImagesOfProduct(List<Product> products)
        {
            var productModels = _mapper.Map<List<ProductModel>>(products);
            // chạy lặp qua products và lấy hình của chúng ra trong product_images
            foreach (var product in productModels)
            {
                var productImages = await _unitOfWorks.ProductImageRepository.GetAll()
                    .Where(si => si.ProductId == product.ProductId)
                    .Select(si => si.image)
                    .ToArrayAsync();

                product.images = productImages;
            }

            return productModels;
        }

        public async Task<List<ProductModel>> GetListImagesOfProductIds(List<int> productIds)
        {
            var listProducts = await _unitOfWorks.ProductRepository
                .FindByCondition(x => productIds.Contains(x.ProductId))
                .ToListAsync();
            var productModels = _mapper.Map<List<ProductModel>>(listProducts);
            // chạy lặp qua products và lấy hình của chúng ra trong product_images
            foreach (var product in productModels)
            {
                var productImages = await _unitOfWorks.ProductImageRepository.GetAll()
                    .Where(si => si.ProductId == product.ProductId)
                    .Select(si => si.image)
                    .ToArrayAsync();

                product.images = productImages;
            }

            return productModels;
        }

        public async Task<GetAllProductPaginationFilter> FilterProductsAsync(ProductFilterRequest req)
        {
            if (req.BranchId <= 0)
            {
                throw new ArgumentException("BrandId là bắt buộc để lọc sản phẩm.");
            }

            IQueryable<Product> query = _unitOfWorks.ProductRepository
                .FindByCondition(p => p.Status == "Active")
                .Include(p => p.Company)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Branch_Products)
                    .ThenInclude(bp => bp.Branch);

            var productIdsInBranch = await _unitOfWorks.Branch_ProductRepository
                .FindByCondition(bp => bp.BranchId == req.BranchId)
                .Select(bp => bp.ProductId)
                .Distinct()
                .ToListAsync();

            query = query.Where(p => productIdsInBranch.Contains(p.ProductId));

            if (!string.IsNullOrEmpty(req.Brand))
            {
                query = query.Where(p =>
                    p.Brand != null &&
                    p.Brand.ToLower().Contains(req.Brand.ToLower())
                );
            }

            if (req.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == req.CategoryId.Value);
            }

            if (req.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= req.MinPrice.Value);
            }

            if (req.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= req.MaxPrice.Value);
            }

            if (!string.IsNullOrEmpty(req.SortBy))
            {
                switch (req.SortBy.ToLower())
                {
                    case "price_asc":
                        query = query.OrderBy(p => p.Price);
                        break;
                    case "price_desc":
                        query = query.OrderByDescending(p => p.Price);
                        break;
                }
            }

            var totalCount = await query.CountAsync();
            int page = req.PageNumber > 0 ? req.PageNumber : 1;
            int pageSize = req.PageSize > 0 ? req.PageSize : 10;
            int totalPage = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedProducts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var productDtos = pagedProducts.Select(p => new ProductDetailDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductDescription = p.ProductDescription,
                Price = p.Price,
                Brand = p.Brand,
                Quantity = p.Quantity,
                StockQuantity = p.Branch_Products?
    .FirstOrDefault(bp => bp.BranchId == req.BranchId)?.StockQuantity ?? 0,
                Discount = p.Discount ?? 0,
                CategoryId = p.CategoryId,
                Dimension = p.Dimension,
                Volume = p.Volume,
                Status = p.Status,
                CategoryName = p.Category?.Name,
                CompanyName = p.Company?.Name,
                SkinTypeSuitable = p.SkinTypeSuitable,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,
                //BrandId = p.Branch_Products?.FirstOrDefault()?.BranchId,
                //BrandName = p.Branch_Products?.FirstOrDefault()?.Branch?.BranchName,
                ProductBranchId = p.Branch_Products?.FirstOrDefault()?.Id,
                Category = new CategoryDetailDto
                {
                    CategoryId = p.Category?.CategoryId ?? 0,
                    Name = p.Category?.Name,
                    Description = p.Category?.Description,
                    Status = p.Category?.Status
                },
                images = p.ProductImages?.Select(i => i.image).ToArray() ?? Array.Empty<string>()
            }).ToList();

            return new GetAllProductPaginationFilter
            {
                data = productDtos,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPage,
                    totalCount = totalCount
                }
            };
        }

        public async Task<ProductDetailDto> GetProductDetailByProductBranchIdAsync(int productBranchId)
        {
            // Tìm branch_product theo Id
            var productBranch = await _unitOfWorks.Branch_ProductRepository
       .FindByCondition(bp => bp.Id == productBranchId)
       .Include(bp => bp.Product)
           .ThenInclude(p => p.Company)
       .Include(bp => bp.Product)
           .ThenInclude(p => p.Category)
       .Include(bp => bp.Product)
           .ThenInclude(p => p.ProductImages)
       .Include(bp => bp.Branch)
       .FirstOrDefaultAsync();


            if (productBranch == null || productBranch.Product == null)
            {
                return null;
            }

            var p = productBranch.Product;

            // Map sang DTO
            var productDto = new ProductDetailDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductDescription = p.ProductDescription,
                Price = p.Price,
                Brand = p.Brand,
                Quantity = p.Quantity,
                StockQuantity = p.Branch_Products?.FirstOrDefault(bp => bp.Id == productBranchId)?.StockQuantity ?? 0,
                Discount = p.Discount ?? 0,
                CategoryId = p.CategoryId,
                Dimension = p.Dimension,
                Volume = p.Volume,
                Status = p.Status,
                CategoryName = p.Category?.Name,
                CompanyName = p.Company?.Name,
                SkinTypeSuitable = p.SkinTypeSuitable,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,
                //BrandId = p.Branch_Products?.FirstOrDefault(bp => bp.Id == productBranchId)?.BranchId,
               // BrandName = p.Branch_Products?.FirstOrDefault(bp => bp.Id == productBranchId)?.Branch?.BranchName,
                ProductBranchId = productBranchId,
                Category = new CategoryDetailDto
                {
                    CategoryId = p.Category?.CategoryId ?? 0,
                    Name = p.Category?.Name,
                    Description = p.Category?.Description,
                    Status = p.Category?.Status
                },
                images = p.ProductImages?.Select(i => i.image).ToArray() ?? Array.Empty<string>(),
                Branch = _mapper.Map<BranchDTO>(productBranch.Branch)
            };

            return productDto;
        }


        public async Task<bool> AssignOrUpdateProductToBranchAsync(AssignProductToBranchRequest request)
        {
            var existing = await _unitOfWorks.Branch_ProductRepository
                .FindByCondition(bp => bp.ProductId == request.ProductId && bp.BranchId == request.BranchId)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.StockQuantity = request.StockQuantity;
                _unitOfWorks.Branch_ProductRepository.Update(existing);
            }
            else
            {
                var newEntry = new Branch_Product
                {
                    ProductId = request.ProductId,
                    BranchId = request.BranchId,
                    StockQuantity = request.StockQuantity
                };

                await _unitOfWorks.Branch_ProductRepository.AddAsync(newEntry);
            }

            await _unitOfWorks.SaveChangesAsync();
            return true;
        }
    }
}



