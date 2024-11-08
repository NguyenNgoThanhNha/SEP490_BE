using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Dtos;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("get-all-products")]
        public async Task<IActionResult> Get([FromQuery] int page = 1)
        {
            var products = await _productService.GetAllProduct(page);
            return Ok(ApiResult<GetAllProductPaginationResponse>.Succeed(new GetAllProductPaginationResponse()
            {
                data = products.data,
                pagination = products.pagination
            }));
        }


        [HttpGet("filter")]
        public async Task<IActionResult> FilterProductsAsync([FromQuery] string? productName, [FromQuery] string? productDescription, [FromQuery] decimal? price, [FromQuery] int? quantity, [FromQuery] decimal? discount, [FromQuery] string? categoryName, [FromQuery] string? companyName)
        {
            try
            {
                // Gọi service để thực hiện logic lọc bất đồng bộ
                var filteredProducts = await _productService.FilterProductAsync(productName, productDescription, price, quantity, discount, categoryName, companyName);

                // Kiểm tra nếu danh sách kết quả rỗng hoặc null
                if (filteredProducts == null || !filteredProducts.Any())
                {
                    return NotFound("No products found matching the filter criteria.");
                }

                var result = filteredProducts.Select(d => new ProductDto
                {
                    ProductId = d.ProductId,
                    ProductName = d.ProductName,
                    ProductDescription = d.ProductDescription,
                    Price = d.Price,
                    Quantity = d.Quantity,
                    Discount = d.Discount,
                    CategoryName = d.Category?.Name,
                    CompanyName = d.Company?.Name,                    
                }).ToList();





                return Ok(ApiResult<List<ProductDto>>.Succeed(result));
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có (tùy chọn: có thể log lại với Serilog, NLog, hoặc bất kỳ framework logging nào)
                Console.WriteLine($"An error occurred: {ex.Message}");

                // Trả về mã lỗi 500 (Internal Server Error) và thông báo lỗi
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
