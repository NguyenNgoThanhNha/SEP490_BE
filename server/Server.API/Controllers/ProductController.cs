using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.API.Extensions;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Data.Entities;
using System.Linq.Expressions;

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

        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(string? productName,
                        string? description,
                        string? categoryName,
                        string? companyName,
                        decimal? price,
                        decimal? endPrice,
                        int? filterTypePrice = 0,
                        int pageIndex = 0,
                        int pageSize = 10)
        {
            Expression<Func<Product, bool>> filter = c => (string.IsNullOrEmpty(productName) || c.ProductName.ToLower().Contains(productName.ToLower()))
                && (string.IsNullOrEmpty(description) || c.ProductDescription.ToLower().Contains(description.ToLower()))
                && (string.IsNullOrEmpty(categoryName) || c.Category.Name.ToLower().Contains(categoryName.ToLower()))
                && (string.IsNullOrEmpty(companyName) || c.Company.Name.ToLower().Contains(companyName.ToLower()));

            Expression<Func<Product, bool>> priceFilter = null;
            if (price != null && price > 0)
            {
                if (filterTypePrice == 0 && (endPrice != null && endPrice > 0)) // khoảng
                {
                    priceFilter = c => c.Price >= price && c.Price <= endPrice;
                }
                else if (filterTypePrice == 1) // nhỏ hơn
                {
                    priceFilter = c => c.Price <= price;
                }
                else  // lớn hơn
                {
                    priceFilter = c => c.Price >= price;
                }
                filter = filter.And(priceFilter);
            }

            var response = await _productService.GetListAsync(
                filter: filter,
                includeProperties: "Category,Company",
                pageIndex: pageIndex,
                pageSize: pageSize);

            return Ok(new ApiResult<Pagination<Product>>
            {
                Success = true,
                Result = response
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto productCreateDto)
        {

            if (productCreateDto == null)
            {
                return BadRequest("Invalid product data.");
            }

            try
            {

                var result = await _productService.CreateProductAsync(productCreateDto);

                if (result.Success)
                {

                    return CreatedAtAction(nameof(GetProductById), new { productId = result.Result.ProductId }, result);
                }
                else
                {

                    return BadRequest(result.Result?.ProductName);
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductById(int productId)
        {

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
            {

                return NotFound($"Product with ID {productId} not found.");
            }


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

                var filteredProducts = await _productService.FilterProductAsync(productName, productDescription, price, quantity, discount, categoryName, companyName);


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





                return Ok(ApiResult<FilterProductResponse>.Succeed(new FilterProductResponse()
                {
                    data = result
                }));
            }
            catch (Exception ex)
            {

                Console.WriteLine($"An error occurred: {ex.Message}");


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


            var result = await _productService.UpdateProductAsync(productId, productUpdateDto);


            if (!result.Success)
            {
                return BadRequest(ApiResult<ProductDetailDto>.Error(null));
            }


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


            return Ok(ApiResult<ProductDetailDto>.Succeed(productDetailDto));
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            try
            {
                bool result = await _productService.DeleteProductAsync(productId);
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
