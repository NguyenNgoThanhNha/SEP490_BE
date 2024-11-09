using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Models;
using Server.Business.Services;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Server.Business.Services
{
    public class ProductService
    {
        private readonly AppDbContext _context;
        private readonly UnitOfWorks unitOfWorks;
        private readonly IMapper _mapper;
        public ProductService(AppDbContext context, UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _context = context;
            this.unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }


        public async Task<ApiResult<Product>> CreateProductAsync(ProductCreateDto productCreateDto)
        {
            try
            {
                if (productCreateDto == null)
                {
                    return ApiResult<Product>.Error(null);
                }

                if (productCreateDto.Price <= 0 || productCreateDto.Quantity <= 0 || productCreateDto.Discount < 0)
                {
                    return ApiResult<Product>.Error(new Product { ProductName = "Invalid Price, Quantity, or Discount" });
                }

                var categoryExists = await _context.Categorys.AnyAsync(c => c.CategoryId == productCreateDto.CategoryId);
                var companyExists = await _context.Companies.AnyAsync(c => c.CompanyId == productCreateDto.CompanyId);

                if (!categoryExists)
                {
                    return ApiResult<Product>.Error(new Product { ProductName = "Category does not exist" });
                }

                if (!companyExists)
                {
                    return ApiResult<Product>.Error(new Product { ProductName = "Company does not exist" });
                }


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
                    UpdatedDate = DateTime.Now
                };


                await _context.Products.AddAsync(newProduct);
                await _context.SaveChangesAsync();


                return ApiResult<Product>.Succeed(newProduct);
            }
            catch (Exception ex)
            {

                return ApiResult<Product>.Error(new Product { ProductName = $"Error: {ex.Message}" });
            }
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            try
            {

                var product = await _context.Products
                                             .Include(d => d.Category)
                                             .Include(d => d.Company)
                                             .FirstOrDefaultAsync(p => p.ProductId == productId);

                return product;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error occurred: {ex.Message}");
                return null;
            }
        }

        public async Task<GetAllProductPaginationResponse> GetAllProduct(int page)
        {
            try
            {
                const int pageSize = 4;

                var products = await unitOfWorks.ProductRepository.GetAll()
                    .Include(p => p.Category)
                    .Include(p => p.Company)
                    .OrderByDescending(x => x.ProductId)
                    .ToListAsync();

                var totalCount = products.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                var productModels = _mapper.Map<List<ProductModel>>(pagedProducts);


                foreach (var product in productModels)
                {
                    product.CategoryName = product.CategoryName;
                    product.CompanyName = product.CompanyName;
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
            catch (Exception ex)
            {
                throw new Exception("Error retrieving products", ex);
            }
        }

        public async Task<List<Product>> FilterProductAsync(
    string? productName,
    string? productDescription,
    decimal? price,
    int? quantity,
    decimal? discount,
    string? categoryName,
    string? companyName)
        {
            try
            {

                var query = _context.Products.Include(d => d.Category).Include(d => d.Company).AsQueryable();


                if (!string.IsNullOrEmpty(productName))
                {
                    string lowerName = productName.ToLower();

                    query = query.Where(d => d.ProductName.ToLower().Contains(lowerName));
                }


                if (!string.IsNullOrEmpty(productDescription))
                {
                    string lowerDescription = productDescription.ToLower();

                    query = query.Where(d => d.ProductDescription.ToLower().Contains(lowerDescription));
                }

                if (!string.IsNullOrEmpty(categoryName))
                {
                    string lowerCategoryName = categoryName.ToLower();
                    query = query.Where(d => d.Category != null && d.Category.Name.ToLower().Contains(lowerCategoryName));
                }


                if (!string.IsNullOrEmpty(companyName))
                {
                    string lowerCompanyName = companyName.ToLower();
                    query = query.Where(d => d.Company != null && d.Company.Name.ToLower().Contains(lowerCompanyName));
                }


                if (price.HasValue)
                {
                    query = query.Where(d => d.Price == price);
                }


                if (discount.HasValue)
                {
                    query = query.Where(d => d.Discount == discount);
                }


                if (quantity.HasValue)
                {
                    query = query.Where(d => d.Quantity == quantity);
                }


                return await query.ToListAsync();
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error occurred: {ex.Message}");


                return new List<Product>();
            }
        }


        public async Task<ApiResult<Product>> UpdateProductAsync(int productId, ProductUpdateDto productUpdateDto)
        {
            try
            {

                if (productUpdateDto == null)
                {
                    return ApiResult<Product>.Error(null);
                }


                if (productUpdateDto.Price <= 0 || productUpdateDto.Quantity <= 0 || productUpdateDto.Discount < 0)
                {
                    return ApiResult<Product>.Error(new Product { ProductName = "Invalid Price, Quantity, or Discount" });
                }


                var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
                if (existingProduct == null)
                {
                    return ApiResult<Product>.Error(new Product { ProductName = "Product not found" });
                }


                var categoryExists = await _context.Categorys.AnyAsync(c => c.CategoryId == productUpdateDto.CategoryId);
                var companyExists = await _context.Companies.AnyAsync(c => c.CompanyId == productUpdateDto.CompanyId);

                if (!categoryExists)
                {
                    return ApiResult<Product>.Error(new Product { ProductName = "Category does not exist" });
                }

                if (!companyExists)
                {
                    return ApiResult<Product>.Error(new Product { ProductName = "Company does not exist" });
                }


                existingProduct.ProductName = productUpdateDto.ProductName;
                existingProduct.ProductDescription = productUpdateDto.ProductDescription;
                existingProduct.Price = productUpdateDto.Price;
                existingProduct.Quantity = productUpdateDto.Quantity;
                existingProduct.Discount = productUpdateDto.Discount;
                existingProduct.CategoryId = productUpdateDto.CategoryId;
                existingProduct.CompanyId = productUpdateDto.CompanyId;
                existingProduct.UpdatedDate = DateTime.Now;


                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();


                return ApiResult<Product>.Succeed(existingProduct);
            }
            catch (Exception ex)
            {

                return ApiResult<Product>.Error(new Product { ProductName = $"Error: {ex.Message}" });
            }
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {

            var product = await _context.Products
                .Include(p => p.Branch_Products)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");


            var hasLinkedServices = await _context.Services
                .AnyAsync(s => s.CategoryId == product.CategoryId);

            if (hasLinkedServices)
                throw new InvalidOperationException("Cannot delete product as its category is linked to a service.");


            _context.Branch_Products.RemoveRange(product.Branch_Products);


            _context.Products.Remove(product);


            await _context.SaveChangesAsync();
            return true;
        }
    }
}



