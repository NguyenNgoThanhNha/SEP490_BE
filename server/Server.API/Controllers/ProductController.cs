using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Data.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly AppDbContext _context;
        public ProductController(ProductService productService, AppDbContext context)
        {
            _productService = productService;
            _context = context;
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto productCreateDto)
        {
            // Kiểm tra nếu ProductDto là null hoặc không hợp lệ
            if (productCreateDto == null)
            {
                return BadRequest("Invalid product data.");
            }

            try
            {
                // Gọi phương thức từ ProductService để tạo sản phẩm
                var result = await _productService.CreateProductAsync(productCreateDto);

                if (result.Success)
                {
                    // Trả về kết quả thành công với sản phẩm mới
                    return CreatedAtAction(nameof(GetProductById), new { id = result.Result.ProductId }, result);
                }
                else
                {
                    // Trả về thông báo lỗi nếu có
                    return BadRequest(result.Result?.ProductName); // Hoặc trường thông báo lỗi khác từ Product
                }
            }
            catch (Exception ex)
            {
                // Trả về lỗi nếu có ngoại lệ
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            // Gọi phương thức từ ProductService để lấy sản phẩm theo ID
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                // Nếu không tìm thấy sản phẩm, trả về mã lỗi 404 (Not Found)
                return NotFound($"Product with ID {id} not found.");
            }

            // Trả về thông tin sản phẩm nếu tìm thấy
            var productDto = new ProductDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                Price = product.Price,
                Quantity = product.Quantity,
                Discount = product.Discount,
                CategoryId = product.CategoryId,
                CompanyId = product.CompanyId,
                CategoryName = product.Category?.Name,
                CompanyName = product.Company?.Name,
                CreatedDate = product.CreatedDate,
                UpdatedDate = product.UpdatedDate
            };

            return Ok(productDto);
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

        [HttpPut("update{productId}")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] ProductUpdateDto productUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResult<string>.Error("Invalid model state"));
            }

            // Gọi đến service để thực hiện cập nhật sản phẩm
            var result = await _productService.UpdateProductAsync(productId, productUpdateDto);

            // Kiểm tra nếu cập nhật thất bại, trả về lỗi
            if (!result.Success)
            {
                return BadRequest(ApiResult<ProductDetailDto>.Error(null));
            }

            // Tạo đối tượng ProductDetailDto từ kết quả trả về
            var product = result.Result;
            var productDetailDto = new ProductDetailDto
            {
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                Price = product.Price,
                Quantity = product.Quantity,
                Discount = (decimal)product.Discount,
                CategoryId = product.CategoryId,
                CompanyId = product.CompanyId,
                CategoryName = await _context.Categorys
                                             .Where(c => c.CategoryId == product.CategoryId)
                                             .Select(c => c.Name)
                                             .FirstOrDefaultAsync(),
                CompanyName = await _context.Companies
                                             .Where(c => c.CompanyId == product.CompanyId)
                                             .Select(c => c.Name)
                                             .FirstOrDefaultAsync(),
                CreatedDate = product.CreatedDate,
                UpdatedDate = product.UpdatedDate
            };

            // Trả về kết quả thành công với ProductDetailDto
            return Ok(ApiResult<ProductDetailDto>.Succeed(productDetailDto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                bool result = await _productService.DeleteProductAsync(id);
                if (result)
                {
                    return Ok(new { message = "Product deleted successfully." });
                }
                else
                {
                    return NotFound(new { message = "Product not found." });
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
