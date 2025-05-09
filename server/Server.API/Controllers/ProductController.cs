﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Server.API.Extensions;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Models;
using Server.Business.Services;
using Server.Data.Entities;
using System.Linq.Expressions;

namespace Server.API.Controllers
{
    // [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly BranchService _branchService;
        private readonly AppDbContext _context;
        private readonly IElasticClient _elasticClient;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ElasticService<ProductDto> _elasticService;

        public ProductController(ProductService productService, BranchService branchService,
            IElasticClient elasticClient, IWebHostEnvironment hostingEnvironment)
        {
            _productService = productService;
            _branchService = branchService;
            _elasticClient = elasticClient;
            _hostingEnvironment = hostingEnvironment;
            _elasticService = new ElasticService<ProductDto>(_elasticClient, "products");
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(
            string? productName,
            string? description,
            string? categoryName,
            string? companyName,
            decimal? price,
            decimal? endPrice,
            int? filterTypePrice = 0,
            int pageIndex = 0,
            int pageSize = 10)
        {
            Expression<Func<Product, bool>> filter = p =>
                (string.IsNullOrEmpty(productName) || p.ProductName.ToLower().Contains(productName.ToLower()))
                && (string.IsNullOrEmpty(description) || p.ProductDescription.ToLower().Contains(description.ToLower()))
                && (string.IsNullOrEmpty(categoryName) || p.Category.Name.ToLower().Contains(categoryName.ToLower()))
                && (string.IsNullOrEmpty(companyName) || p.Company.Name.ToLower().Contains(companyName.ToLower()))
                && p.Status == "Active";

            if (price.HasValue && price > 0)
            {
                if (filterTypePrice == 0 && endPrice.HasValue && endPrice > 0) // khoảng
                {
                    filter = filter.And(p => p.Price >= price && p.Price <= endPrice);
                }
                else if (filterTypePrice == 1) // nhỏ hơn
                {
                    filter = filter.And(p => p.Price <= price);
                }
                else // lớn hơn
                {
                    filter = filter.And(p => p.Price >= price);
                }
            }

            var products = await _productService.GetListAsync(
                filter: filter,
                includeProperties: "Category,Company", // Include bảng liên quan
                pageIndex: pageIndex,
                pageSize: pageSize
            );

            if (products == null || !products.Data.Any())
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không tìm thấy sản phẩm nào!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Lấy danh sách sản phẩm thành công!",
                data = products
            }));
        }

        [HttpGet("elasticsearch")]
        public async Task<IActionResult> ElasticSearch(string? keyword)
        {
            var productList = new List<ProductDto>();
            if (!string.IsNullOrEmpty(keyword))
            {
                productList = (await _elasticService.SearchAsync(keyword)).ToList();
            }
            else
                productList = (await _elasticService.GetAllAsync()).ToList();

            return Ok(ApiResponse.Succeed(productList));
        }

        [HttpPost("create-elastic")]
        public async Task<IActionResult> CreateElastic(ProductDto model)
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


        //[Authorize(Roles = "Admin, Manager")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductCreateDto productCreateDto)
        {
            // Kiểm tra tính hợp lệ của Model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            // Gọi service để tạo sản phẩm
            var result = await _productService.CreateProductAsync(productCreateDto);

            // Kiểm tra nếu có lỗi
            if (!result.Success)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = result.Result?.message
                }));
            }

            // Trả về phản hồi thành công
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Sản phẩm đã được tạo thành công!",
                data = result.Result?.data
            }));
        }


        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductById(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Sản phẩm có ID {productId} không tìm thấy."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Lấy sản phẩm thành công!",
                data = product
            }));
        }


        [HttpGet("get-all-products")]
        public async Task<IActionResult> Get([FromQuery] int page = 1, int pageSize = 6)
        {
            try
            {
                var products = await _productService.GetAllProduct(page, pageSize);

                // Kiểm tra nếu không có dữ liệu
                if (products.data == null || !products.data.Any())
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Không tìm thấy sản phẩm nào."
                    }));
                }

                products.message = "Lấy sản phẩm thành công.";
                return Ok(ApiResult<GetAllProductPaginationResponse>.Succeed(products));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Đã xảy ra lỗi khi truy xuất sản phẩm: {ex.Message}"
                }));
            }
        }

        [Authorize(Roles = "Admin, Manager,Staff")]
        [HttpGet("check-quantity-branch")]
        public async Task<IActionResult> CheckProductInBranchAsync(int productId, int branchId)
        {
            if (productId == 0 || branchId == 0)
                return BadRequest(ApiResponse.Error("Vui lòng nhập thông tin đầy đủ"));
            if (await _productService.GetProductByIdAsync(productId) == null)
            {
                return BadRequest(ApiResponse.Error("Không tìm thấy sản phẩm"));
            }

            if (await _branchService.GetBranchAsync(productId) == null)
            {
                return BadRequest(ApiResponse.Error("Không tìm thấy chi nhánh"));
            }

            var productBranch = await _productService.GetProductInBranchAsync(productId, branchId);
            if (productBranch == null)
            {
                return BadRequest(ApiResponse.Error("Sản phẩm này không tồn tại trong chi nhánh"));
            }

            return Ok(ApiResponse.Succeed(productBranch));
        }


        //[Authorize(Roles = "Admin, Manager")]
        [HttpPut("update/{productId}")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] ProductUpdateDto productUpdateDto)
        {
            // Kiểm tra tính hợp lệ của ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            // Gọi service để cập nhật sản phẩm
            var result = await _productService.UpdateProductAsync(productId, productUpdateDto);

            // Xử lý nếu kết quả thất bại
            if (!result.Success)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = result.Result?.message // Lấy thông báo lỗi từ service
                }));
            }

            // Trả về phản hồi thành công
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Product updated successfully!",
                data = result.Result?.data // Dữ liệu sản phẩm sau khi cập nhật
            }));
        }


        [Authorize(Roles = "Admin, Manager")]
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(productId);

                var msg = result?.message?.ToLower() ?? "";
                if (msg.Contains("not found") || msg.Contains("failed") || msg.Contains("error"))
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = result.message
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = result.message,
                    data = productId.ToString()
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi hệ thống: {ex.Message}"
                }));
            }
        }


        [HttpGet("top-5-best-sellers")]
        public async Task<IActionResult> GetTop5BestSellers([FromQuery] int branchId)
        {
            try
            {
                var bestSellers = await _productService.GetTop5BestSellersByBranchAsync(branchId);

                if (bestSellers == null || !bestSellers.Any())
                {
                    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                    {
                        message = "Không tìm thấy sản phẩm bán chạy nào cho chi nhánh này."
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    data = bestSellers,
                    message = "Lấy danh sách Top 5 sản phẩm bán chạy thành công!"
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi hệ thống: {ex.Message}"
                }));
            }
        }


        [HttpGet("check-input-gross")]
        public async Task<IActionResult> CheckInputGross(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return BadRequest();
            }

            var result = await _productService.CheckInputHasGross(input);
            return Ok(ApiResponse.Succeed(result));
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterProducts([FromQuery] ProductFilterRequest req)
        {
            // Kiểm tra BrandId hợp lệ (nếu bạn chưa validate từ DTO)
            if (req.BranchId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "BranchId là bắt buộc để lọc sản phẩm."
                });
            }

            var result = await _productService.FilterProductsAsync(req);

            return Ok(new
            {
                success = true,
                result = new
                {
                    message = "Đã lấy sản phẩm thành công.",
                    data = result.data,
                    pagination = result.pagination
                }
            });
        }


        [HttpGet("detail-by-productBranchId")]
        public async Task<IActionResult> GetProductDetailByBranch([FromQuery] int productBranchId)
        {
            var result = await _productService.GetProductDetailByProductBranchIdAsync(productBranchId);

            if (result == null)
            {
                return Ok(new
                {
                    success = true,
                    result = new
                    {
                        message =
                            "Không tìm thấy sản phẩm tương ứng với ProductBranchId. Đảm bảo đã nhập ProductBranchId đúng.",
                        data = (object)null
                    }
                });
            }

            return Ok(new
            {
                success = true,
                result = new
                {
                    message = "Lấy thông tin sản phẩm thành công.",
                    data = result
                }
            });
        }


        [HttpPost("assign-to-branch")]
        public async Task<IActionResult> AssignProductToBranch([FromBody] AssignProductToBranchRequest request)
        {
            var result = await _productService.AssignOrUpdateProductToBranchAsync(request);

            if (result)
            {
                return Ok(new
                {
                    success = true,
                    result = new
                    {
                        message = "Gán sản phẩm vào chi nhánh thành công."
                    }
                });
            }

            return BadRequest(new
            {
                success = false,
                message = "Thao tác thất bại."
            });
        }

        [HttpGet("sold-products-by-branch")]
        public async Task<IActionResult> GetSoldProductsByBranch([FromQuery] int branchId)
        {
            var result = await _productService.GetSoldProductsByBranchAsync(branchId);

            if (result == null || result.Items == null || !result.Items.Any())
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không có dữ liệu sản phẩm đã bán cho chi nhánh này."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Lấy danh sách sản phẩm bán theo chi nhánh thành công!",
                data = result
            }));
        }

        [HttpDelete("elastic-clear")]
        public async Task<IActionResult> ClearElasticProduct()
        {
            try
            {
                await _elasticService.DeleteAllDocumentsAsync();
                return Ok(ApiResponse.Succeed("Đã xóa toàn bộ dữ liệu Elasticsearch của sản phẩm."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Lỗi khi xóa dữ liệu Elasticsearch: {ex.Message}"));
            }
        }


        [HttpGet("branches-has-product")]
        public async Task<IActionResult> GetBranchesHasProduct(int productId)
        {
            var branches = await _productService.GetBranchesHasProduct(productId);

            return Ok(ApiResult<GetBranchesHasProductResponse<GetBranchesHasProduct>>.Succeed(branches));
        }
    }
}