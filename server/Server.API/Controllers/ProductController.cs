﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.API.Controllers.Gaurd;
using Server.API.Extensions;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
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

        public ProductController(ProductService productService, BranchService branchService)
        {
            _productService = productService;
            _branchService = branchService;
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


        [Authorize(Roles = "Admin, Manager")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto productCreateDto)
        {
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
            if (!result.Success) // Nếu `Success` là false
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
        public async Task<IActionResult> GetAllProducts(int page = 1)
        {
            var products = await _productService.GetAllProduct(page);

            if (products == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "No products found!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Get all products successfully!",
                data = products
            }));
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

    }
}
