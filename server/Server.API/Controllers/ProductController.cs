using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Server.API.Extensions;
using Server.Business.Commons;
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
        public ProductController(ProductService productService, BranchService branchService, IElasticClient elasticClient, IWebHostEnvironment hostingEnvironment)
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
                    message = "No products found!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Get products successfully!",
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


        [Authorize(Roles = "Admin, Manager")]
        [HttpPost("create")]
        //public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto productCreateDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.Values
        //            .SelectMany(v => v.Errors)
        //            .Select(e => e.ErrorMessage)
        //            .ToList();

        //        return BadRequest(ApiResult<List<string>>.Error(errors));
        //    }

        //    // Gọi service để tạo sản phẩm
        //    var result = await _productService.CreateProductAsync(productCreateDto);

        //    // Kiểm tra nếu có lỗi
        //    if (!result.Success) // Nếu `Success` là false
        //    {
        //        return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
        //        {
        //            message = result.Result?.message
        //        }));
        //    }

        //    // Trả về phản hồi thành công
        //    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
        //    {
        //        message = "Product created successfully!",
        //        data = result.Result?.data
        //    }));
        //}
        //
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
                message = "Product created successfully!",
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
                    message = $"Product with ID {productId} not found."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Get product successfully!",
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
                        message = "No products found."
                    }));
                }

                products.message = "Products retrieved successfully.";
                return Ok(ApiResult<GetAllProductPaginationResponse>.Succeed(products));
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"An error occurred while retrieving products: {ex.Message}"
                }));
            }
        }

        [Authorize(Roles = "Admin, Manager,Staff")]
        [HttpGet("check-quantity-branch")]
        public async Task<IActionResult> CheckProductInBranchAsync(int productId, int branchId)
        {
            if (productId == 0 || branchId == 0)
                return BadRequest(ApiResponse.Error("Please enter complete information"));
            if (await _productService.GetProductByIdAsync(productId) == null)
            {
                return BadRequest(ApiResponse.Error("Product not found"));
            }
            if (await _branchService.GetBranchAsync(productId) == null)
            {
                return BadRequest(ApiResponse.Error("Branch not found"));
            }

            var productBranch = await _productService.GetProductInBranchAsync(productId, branchId);
            if (productBranch == null)
            {
                return BadRequest(ApiResponse.Error("This product not exist in branch"));
            }

            return Ok(ApiResponse.Succeed(productBranch));
        }


        [Authorize(Roles = "Admin, Manager")]
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
                // Gọi service để thực hiện xóa sản phẩm
                var result = await _productService.DeleteProductAsync(productId);

                // Kiểm tra kết quả
                if (result == null || string.IsNullOrEmpty(result.message))
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Product not found or cannot be deleted."
                    }));
                }
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Product deleted successfully!"
                }));
            }
            catch (InvalidOperationException ex)
            {
                // Xử lý lỗi liên quan đến ràng buộc dữ liệu
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = ex.Message
                }));
            }
            catch (KeyNotFoundException ex)
            {
                // Xử lý lỗi nếu không tìm thấy sản phẩm
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = ex.Message
                }));
            }
            catch (Exception ex)
            {
                // Xử lý lỗi chung
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"An error occurred while deleting the product: {ex.Message}"
                }));
            }
        }

        //[HttpGet("top5-bestsellers")]
        //public async Task<IActionResult> GetTop5BestSellers()
        //{
        //    try
        //    {
        //        // Gọi Service để lấy dữ liệu
        //        var bestSellers = await _productService.GetTop5BestSellersAsync();

        //        // Kiểm tra kết quả
        //        if (bestSellers == null || !bestSellers.Any())
        //        {
        //            return NotFound(new
        //            {
        //                Message = "Không tìm thấy sản phẩm bán chạy nào."
        //            });
        //        }

        //        // Trả về dữ liệu thành công
        //        return Ok(new
        //        {
        //            Message = "Lấy danh sách Top 5 sản phẩm bán chạy thành công!",
        //            Data = bestSellers
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Xử lý lỗi hệ thống
        //        return StatusCode(500, new
        //        {
        //            Message = $"Lỗi hệ thống: {ex.Message}"
        //        });
        //    }
        //}

        [HttpGet("top5-bestsellers")]
        public async Task<IActionResult> GetTop5BestSellers()
        {
            try
            {
                // Gọi Service để lấy dữ liệu
                var bestSellers = await _productService.GetTop5BestSellersAsync();

                // Kiểm tra kết quả
                if (bestSellers == null || !bestSellers.Any())
                {
                    return NotFound(new
                    {
                        Message = "Không tìm thấy sản phẩm bán chạy nào."
                    });
                }

                // Trả về dữ liệu thành công
                return Ok(new
                {
                    Message = "Lấy danh sách Top 5 sản phẩm bán chạy thành công!",
                    Data = bestSellers
                });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi hệ thống
                return StatusCode(500, new
                {
                    Message = $"Lỗi hệ thống: {ex.Message}"
                });
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
        public async Task<IActionResult> FilterProducts([FromQuery] ProductFilterRequest request)
        {
            try
            {
                // Gọi service lọc sản phẩm
                var result = await _productService.FilterProductsAsync(request);

                // Ép kiểu result.Result sang ApiResponse để lấy dữ liệu
                var apiResponse = result?.Result as ApiResponse;

                var data = apiResponse?.data;

                // Kiểm tra nếu không có sản phẩm nào
                if (data == null || (data is IEnumerable<object> list && !list.Any()))
                {
                    var emptyResponse = ApiResponse.Error("No products found based on the filter criteria");
                    return Ok(ApiResult<ApiResponse>.Succeed(emptyResponse));
                }

                // Trả về kết quả thành công
                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse.Error($"Something went wrong: {ex.Message}");
                return Ok(ApiResult<ApiResponse>.Succeed(errorResponse));
            }
        }




        //[HttpGet("filter")]
        //public async Task<IActionResult> FilterProducts([FromQuery] ProductFilterRequest request)
        //{
        //    try
        //    {
        //        // Gọi dịch vụ để lọc sản phẩm với các tham số có sẵn trong request
        //        var result = await _productService.FilterProductsAsync(request);

        //        // Kiểm tra nếu không có kết quả hoặc có lỗi trong quá trình lọc
        //        if (result == null || !result.Success)
        //        {
        //            return BadRequest(ApiResult<object>.Error(null, "No products found based on the filter criteria"));
        //        }

        //        // Trả về kết quả lọc sản phẩm thành công
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResult<object>.Error(null, $"Something went wrong: {ex.Message}"));
        //    }
        //}

    }
}
