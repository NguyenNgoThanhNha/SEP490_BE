﻿using AutoMapper;
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
using System.Linq;
using System.Linq.Expressions;
using Server.Business.Exceptions;
using Server.Data;

namespace Server.Business.Services
{
    public class ProductService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly CloudianryService _cloudianryService;
        private readonly IAIMLService _gptService;

        public ProductService(UnitOfWorks unitOfWorks, IMapper mapper, CloudianryService cloudianryService,
            IAIMLService gptService)
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
                    Dimension = p.Dimension,
                    Quantity = p.Quantity,
                    Status = p.Status,
                    CompanyId = p.Company != null ? p.Company.CompanyId : 0, // Kiểm tra null
                    CompanyName = p.Company != null ? p.Company.Name : string.Empty, // Kiểm tra null
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,
                    CategoryId = p.CategoryId,
                    Category = p.Category != null
                        ? new CategoryDto
                        {
                            CategoryId = p.Category.CategoryId,
                            Name = p.Category.Name,
                            Description = p.Category.Description,
                            Status = p.Category.Status,
                            ImageUrl = p.Category.ImageUrl,
                            CreatedDate = p.Category.CreatedDate,
                            UpdatedDate = p.Category.UpdatedDate
                        }
                        : null, // Nếu Category null, gán null
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
            if (productCreateDto == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Please enter complete information."
                });
            }
            
            if (productCreateDto.Price <= 0 || productCreateDto.Quantity <= 0)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Invalid Price, Quantity."
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
                CategoryId = productCreateDto.CategoryId,
                CompanyId = productCreateDto.CompanyId,
                Brand = productCreateDto.Brand,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Dimension = productCreateDto.Dimension,
                Status = ObjectStatus.Active.ToString()
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
                    createdProduct.Status,
                    createdProduct.CategoryId,
                    CategoryName = createdProduct.Category?.Name,
                    createdProduct.CompanyId,
                    CompanyName = createdProduct.Company?.Name,
                    createdProduct.Dimension,
                    createdProduct.CreatedDate,
                    createdProduct.UpdatedDate,
                    ImageUrl = imageUrl,
                    createdProduct.Brand
                }
            });
        }


        public async Task<ProductModel?> GetProductByIdAsync(int productId)
        {
            try
            {
                // Lấy sản phẩm có trạng thái "Active"
                var product = await _unitOfWorks.ProductRepository
                    .FindByCondition(p => p.ProductId == productId && p.Status == ObjectStatus.Active.ToString())
                    .Include(d => d.Category)
                    .Include(d => d.Company)
                    .FirstOrDefaultAsync();

                // Kiểm tra nếu không tìm thấy sản phâm
                if (product == null)
                    return null;

                var productModel = _mapper.Map<ProductModel>(product);

                var serviceImages = await _unitOfWorks.ProductImageRepository
                    .FindByCondition(x => x.ProductId == productModel.ProductId)
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


        //public async Task<GetAllProductPaginationResponse> GetAllProduct(int page, int pageSize)
        //{
        //    // Lấy dữ liệu sản phẩm với Category và áp dụng phân trang trực tiếp trên IQueryable
        //    var query = _unitOfWorks.ProductRepository.GetAll()
        //        .Include(p => p.Category) // Bao gồm thông tin Category
        //        .Include(p => p.Company)
        //        .OrderByDescending(p => p.ProductId);

        //    var totalCount = await query.CountAsync();
        //    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        //    var products = await query
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    // Ánh xạ sang ProductModel
        //    var productModels = _mapper.Map<List<ProductModel>>(products);

        //    // Lấy hình ảnh cho tất cả sản phẩm một lần
        //    var productIds = products.Select(p => p.ProductId).ToList();
        //    var productImages = await _unitOfWorks.ProductImageRepository.GetAll()
        //        .Where(si => productIds.Contains(si.ProductId))
        //        .GroupBy(si => si.ProductId)
        //        .ToDictionaryAsync(g => g.Key, g => g.Select(si => si.image).ToArray());

        //    // Gán hình ảnh và category cho từng sản phẩm
        //    foreach (var product in productModels)
        //    {
        //        product.images = productImages.ContainsKey(product.ProductId)
        //            ? productImages[product.ProductId]
        //            : Array.Empty<string>();
        //    }

        //    return new GetAllProductPaginationResponse
        //    {
        //        data = productModels,
        //        pagination = new Pagination
        //        {
        //            page = page,
        //            totalPage = totalPages,
        //            totalCount = totalCount
        //        }
        //    };
        //}

        public async Task<GetAllProductPaginationResponse> GetAllProduct(int page, int pageSize)
        {
            // Include thêm Branch_Products → Branch & Promotion
            var query = _unitOfWorks.ProductRepository.GetAll()
                .Include(p => p.Category)
                .Include(p => p.Company)
                .Include(p => p.Branch_Products)
                .ThenInclude(bp => bp.Branch)
                .Include(p => p.Branch_Products)
                .ThenInclude(bp => bp.Promotion)
                .OrderByDescending(p => p.ProductId);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var productModels = _mapper.Map<List<ProductModel>>(products);

            // Lấy hình ảnh
            var productIds = products.Select(p => p.ProductId).ToList();
            var productImages = await _unitOfWorks.ProductImageRepository.GetAll()
                .Where(si => productIds.Contains(si.ProductId))
                .GroupBy(si => si.ProductId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(si => si.image).ToArray());

            // Gán hình ảnh, branch, promotion
            for (int i = 0; i < productModels.Count; i++)
            {
                var model = productModels[i];
                var entity = products[i];

                model.images = productImages.ContainsKey(model.ProductId)
                    ? productImages[model.ProductId]
                    : Array.Empty<string>();

                var branchProduct = entity.Branch_Products?.FirstOrDefault();

                if (branchProduct != null)
                {
                    if (branchProduct.Branch != null)
                    {
                        model.Branch = new BranchDTO
                        {
                            BranchId = branchProduct.Branch.BranchId,
                            BranchName = branchProduct.Branch.BranchName,
                            BranchAddress = branchProduct.Branch.BranchAddress,
                            BranchPhone = branchProduct.Branch.BranchPhone,
                            LongAddress = branchProduct.Branch.LongAddress,
                            LatAddress = branchProduct.Branch.LatAddress,
                            Status = branchProduct.Branch.Status,
                            ManagerId = branchProduct.Branch.ManagerId,
                            District = branchProduct.Branch.District,
                            WardCode = branchProduct.Branch.WardCode,
                            CompanyId = branchProduct.Branch.CompanyId,
                            CreatedDate = branchProduct.Branch.CreatedDate,
                            UpdatedDate = branchProduct.Branch.UpdatedDate,
                            ManagerBranch = null,
                            Branch_Promotion = null
                        };
                    }

                    if (branchProduct.Promotion != null)
                    {
                        model.Promotion = new PromotionDTO
                        {
                            PromotionId = branchProduct.Promotion.PromotionId,
                            PromotionName = branchProduct.Promotion.PromotionName,
                            PromotionDescription = branchProduct.Promotion.PromotionDescription,
                            DiscountPercent = branchProduct.Promotion.DiscountPercent,
                            StartDate = branchProduct.Promotion.StartDate,
                            EndDate = branchProduct.Promotion.EndDate,
                            Status = branchProduct.Promotion.Status,
                            Image = branchProduct.Promotion.Image,
                            CreatedDate = branchProduct.Promotion.CreatedDate,
                            UpdatedDate = branchProduct.Promotion.UpdatedDate
                        };
                    }
                }
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

                if (productUpdateDto.Price <= 0 || productUpdateDto.Quantity <= 0)
                {
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Invalid Price, Quantity."
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
                var product = await _unitOfWorks.ProductRepository
                    .FindByCondition(p => p.ProductId == productId)
                    .Include(p => p.Branch_Products)
                    .FirstOrDefaultAsync();

                // Nếu không tìm thấy hoặc đã không còn active
                if (product == null || !string.Equals(product.Status, ObjectStatus.Active.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse.Error("Sản phẩm không tồn tại.");
                }

                product.Status = ObjectStatus.InActive.ToString();
                _unitOfWorks.ProductRepository.Update(product);
                var result = await _unitOfWorks.ProductRepository.Commit();

                if (result > 0)
                {
                    return new ApiResponse
                    {
                        message = "Sản phẩm đã được chuyển sang trạng thái Inactive.",
                        data = product.ProductId
                    };
                }

                return ApiResponse.Error("Không thể cập nhật trạng thái sản phẩm.");
            }
            catch (Exception ex)
            {
                return ApiResponse.Error($"Lỗi khi xóa sản phẩm: {ex.Message}");
            }
        }


        public async Task<List<BranchProductDto>> GetTop5BestSellersByBranchAsync(int branchId)
        {
            // 1. Lấy danh sách OrderDetail có trạng thái Completed
            var orderDetails = await _unitOfWorks.OrderDetailRepository
                .FindByCondition(od => od.Status == "Completed")
                .Include(od => od.Product)
                .ThenInclude(p => p.Branch_Products)
                .ThenInclude(bp => bp.Branch)
                .Include(od => od.Product)
                .ThenInclude(p => p.Company)
                .Include(od => od.Product)
                .ThenInclude(p => p.Category)
                .ToListAsync();

            // 2. Lọc các OrderDetail mà Product có liên kết với BranchId cần tìm
            var filteredOrderDetails = orderDetails
                .Where(od => od.Product.Branch_Products.Any(bp => bp.BranchId == branchId))
                .ToList();

            // 3. Gom nhóm theo ProductId và tính tổng số lượng đã bán tại chi nhánh
            var topSelling = filteredOrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g =>
                {
                    var branchProduct = g.First().Product.Branch_Products
                        .FirstOrDefault(bp => bp.BranchId == branchId);

                    return new
                    {
                        BranchProduct = branchProduct,
                        TotalQuantity = g.Sum(od => od.Quantity)
                    };
                })
                .Where(x => x.BranchProduct != null)
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .Select(x => _mapper.Map<BranchProductDto>(x.BranchProduct))
                .ToList();

            return topSelling;
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
            IQueryable<Product> query = _unitOfWorks.ProductRepository
                .FindByCondition(p => p.Status == "Active")
                .Include(p => p.Company)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Branch_Products)
                .ThenInclude(bp => bp.Branch)
                .Include(p => p.Branch_Products)
                .ThenInclude(bp => bp.Promotion);

            // Lọc theo BranchId nếu có
            if (req.BranchId.HasValue && req.BranchId.Value > 0)
            {
                query = query.Where(p => p.Branch_Products.Any(bp => bp.BranchId == req.BranchId.Value));
            }

            if (!string.IsNullOrEmpty(req.Brand))
            {
                query = query.Where(p =>
                    p.Brand != null &&
                    p.Brand.ToLower().Contains(req.Brand.ToLower()));
            }

            if (!string.IsNullOrEmpty(req.CategoryIds))
            {
                var categoryIdList = req.CategoryIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(idStr => int.TryParse(idStr, out var id) ? id : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToList();

                if (categoryIdList.Any())
                {
                    query = query.Where(p => categoryIdList.Contains(p.CategoryId));
                }
            }

            if (req.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= req.MinPrice.Value);
            }

            if (req.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= req.MaxPrice.Value);
            }

            // Sắp xếp theo yêu cầu
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

            var productDtos = pagedProducts.Select(p =>
            {
                var branchProduct = p.Branch_Products?.FirstOrDefault(bp => bp.BranchId == req.BranchId);

                return new ProductDetailDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    Price = p.Price,
                    Brand = p.Brand,
                    Quantity = p.Quantity,
                    StockQuantity = branchProduct?.StockQuantity ?? 0,
                    CategoryId = p.CategoryId,
                    Dimension = p.Dimension,
                    Status = p.Status,
                    CategoryName = p.Category?.Name,
                    CompanyName = p.Company?.Name,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,
                    ProductBranchId = branchProduct?.Id,
                    Category = new CategoryDetailDto
                    {
                        CategoryId = p.Category?.CategoryId ?? 0,
                        Name = p.Category?.Name,
                        Description = p.Category?.Description,
                        Status = p.Category?.Status
                    },
                    images = p.ProductImages?.Select(i => i.image).ToArray() ?? Array.Empty<string>(),

                    Promotion = branchProduct?.Promotion == null
                        ? null
                        : new PromotionDTO
                        {
                            PromotionId = branchProduct.Promotion.PromotionId,
                            PromotionName = branchProduct.Promotion.PromotionName,
                            PromotionDescription = branchProduct.Promotion.PromotionDescription,
                            DiscountPercent = branchProduct.Promotion.DiscountPercent,
                            StartDate = branchProduct.Promotion.StartDate,
                            EndDate = branchProduct.Promotion.EndDate,
                            Status = branchProduct.Promotion.Status,
                            Image = branchProduct.Promotion.Image,
                            CreatedDate = branchProduct.Promotion.CreatedDate,
                            UpdatedDate = branchProduct.Promotion.UpdatedDate
                        },

                    Branch = branchProduct?.Branch == null
                        ? null
                        : new BranchDTO
                        {
                            BranchId = branchProduct.Branch.BranchId,
                            BranchName = branchProduct.Branch.BranchName,
                            BranchAddress = branchProduct.Branch.BranchAddress,
                            BranchPhone = branchProduct.Branch.BranchPhone,
                            LongAddress = branchProduct.Branch.LongAddress,
                            LatAddress = branchProduct.Branch.LatAddress,
                            Status = branchProduct.Branch.Status,
                            ManagerId = branchProduct.Branch.ManagerId,
                            District = branchProduct.Branch.District,
                            WardCode = branchProduct.Branch.WardCode,
                            CompanyId = branchProduct.Branch.CompanyId,
                            CreatedDate = branchProduct.Branch.CreatedDate,
                            UpdatedDate = branchProduct.Branch.UpdatedDate,
                            ManagerBranch = null,
                            Branch_Promotion = null
                        }
                };
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
                .Include(bp => bp.Promotion)
                .Include(bp => bp.Branch)
                .Include(bp => bp.Product)
                .ThenInclude(p => p.Company)
                .Include(bp => bp.Product)
                .ThenInclude(p => p.Category)
                .Include(bp => bp.Product)
                .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync();

            if (productBranch == null || productBranch.Product == null)
            {
                return null;
            }

            var p = productBranch.Product;

            var productDto = new ProductDetailDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductDescription = p.ProductDescription,
                Price = p.Price,
                Brand = p.Brand,
                Quantity = p.Quantity,
                StockQuantity = productBranch.StockQuantity,
                CategoryId = p.CategoryId,
                Dimension = p.Dimension,
                Status = p.Status,
                CategoryName = p.Category?.Name,
                CompanyName = p.Company?.Name,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,
                ProductBranchId = productBranchId,
                Category = new CategoryDetailDto
                {
                    CategoryId = p.Category?.CategoryId ?? 0,
                    Name = p.Category?.Name,
                    Description = p.Category?.Description,
                    Status = p.Category?.Status
                },
                images = p.ProductImages?.Select(i => i.image).ToArray() ?? Array.Empty<string>(),

                Branch = productBranch.Branch == null
                    ? null
                    : new BranchDTO
                    {
                        BranchId = productBranch.Branch.BranchId,
                        BranchName = productBranch.Branch.BranchName,
                        BranchAddress = productBranch.Branch.BranchAddress,
                        BranchPhone = productBranch.Branch.BranchPhone,
                        LongAddress = productBranch.Branch.LongAddress,
                        LatAddress = productBranch.Branch.LatAddress,
                        Status = productBranch.Branch.Status,
                        ManagerId = productBranch.Branch.ManagerId,
                        District = productBranch.Branch.District,
                        WardCode = productBranch.Branch.WardCode,
                        CompanyId = productBranch.Branch.CompanyId,
                        CreatedDate = productBranch.Branch.CreatedDate,
                        UpdatedDate = productBranch.Branch.UpdatedDate,
                        ManagerBranch = null,
                        Branch_Promotion = null
                    },

                Promotion = productBranch.Promotion == null
                    ? null
                    : new PromotionDTO
                    {
                        PromotionId = productBranch.Promotion.PromotionId,
                        PromotionName = productBranch.Promotion.PromotionName,
                        PromotionDescription = productBranch.Promotion.PromotionDescription,
                        DiscountPercent = productBranch.Promotion.DiscountPercent,
                        StartDate = productBranch.Promotion.StartDate,
                        EndDate = productBranch.Promotion.EndDate,
                        Status = productBranch.Promotion.Status,
                        Image = productBranch.Promotion.Image,
                        CreatedDate = productBranch.Promotion.CreatedDate,
                        UpdatedDate = productBranch.Promotion.UpdatedDate
                    }
            };

            return productDto;
        }


        public async Task<bool> AssignOrUpdateProductToBranchAsync(AssignProductToBranchRequest request)
        {
            var existing = await _unitOfWorks.Branch_ProductRepository
                .FindByCondition(bp => bp.ProductId == request.ProductId && bp.BranchId == request.BranchId)
                .FirstOrDefaultAsync();
            
            if (existing == null && request.StockQuantity <= 0)
            {
                throw new BadRequestException("Số lượng sản phẩm phải lớn hơn 0 khi thêm mới.");
            }

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

        public async Task<ProductSoldResponse> GetSoldProductsByBranchAsync(int branchId)
        {
            // B1: Lấy danh sách ProductId thuộc chi nhánh
            var productIds = await _unitOfWorks.Branch_ProductRepository
                .FindByCondition(bp => bp.BranchId == branchId)
                .Select(bp => bp.ProductId)
                .ToListAsync();

            if (!productIds.Any())
            {
                return new ProductSoldResponse
                {
                    Items = new List<ProductSoldByBranchDto>(),
                    TotalQuantitySold = 0
                };
            }

            // B2: Lấy OrderDetail liên quan đến các sản phẩm trong chi nhánh
            var orderDetails = await _unitOfWorks.OrderDetailRepository
                .FindByCondition(od =>
                    od.ProductId.HasValue &&
                    productIds.Contains(od.ProductId.Value))
                .Include(od => od.Product)
                .ToListAsync();


            // B3: Gom nhóm theo ProductId.Value (ép kiểu rõ ràng)
            var grouped = orderDetails
                .GroupBy(od => od.ProductId!.Value) // ép về int
                .Select(g => new ProductSoldByBranchDto
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product?.ProductName ?? "Unknown",
                    TotalQuantitySold = g.Sum(x => x.Quantity)
                })
                .ToList();

            // B4: Tính tổng tất cả số lượng đã bán
            var totalSold = grouped.Sum(x => x.TotalQuantitySold);

            // B5: Trả kết quả
            return new ProductSoldResponse
            {
                Items = grouped,
                TotalQuantitySold = totalSold
            };
        }

        public async Task<GetBranchesHasProductResponse<GetBranchesHasProduct>> GetBranchesHasProduct(int productId)
        {
            var product = await _unitOfWorks.ProductRepository
                              .FirstOrDefaultAsync(x => x.ProductId == productId)
                          ?? throw new BadRequestException("Không tìm thấy thông tin sản phẩm");

            var branchProducts = await _unitOfWorks.Branch_ProductRepository
                .FindByCondition(x => x.ProductId == product.ProductId)
                .Include(x => x.Branch)
                .Include(x => x.Product)
                .Include(x => x.Promotion)
                .ToListAsync();

            var branchDtos = branchProducts.Select(x => new BranchProductDto
            {
                Id = x.Id,
                Product = _mapper.Map<ProductDto>(x.Product),
                Branch = _mapper.Map<BranchDTO>(x.Branch),
                Promotion = x.Promotion == null
                    ? null
                    : _mapper.Map<PromotionDTO>(x.Promotion),
                Status = x.Status,
                StockQuantity = x.StockQuantity,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate
            }).ToList();

            var result = new GetBranchesHasProduct
            {
                ProductId = productId,
                Branches = branchDtos
            };

            return new GetBranchesHasProductResponse<GetBranchesHasProduct>
            {
                Message = "Lấy danh sách chi nhánh có sản phẩm thành công",
                Data = result
            };
        }
    }
}