using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        public async Task<GetAllProductPaginationResponse> GetAllProduct(int page)
        {
            try
            {
                const int pageSize = 4;

                var products = await unitOfWorks.ProductRepository.GetAll()
                    .Include(p => p.Category)  // Bao gồm Category
                    .Include(p => p.Company)   // Bao gồm Company
                    .OrderByDescending(x => x.ProductId)
                    .ToListAsync();

                var totalCount = products.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                var productModels = _mapper.Map<List<ProductModel>>(pagedProducts);

                // Ánh xạ CategoryName và CompanyName
                foreach (var product in productModels)
                {
                    product.CategoryName = product.CategoryName;  // Nếu có Category
                    product.CompanyName = product.CompanyName;    // Nếu có Company
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
    }
}



