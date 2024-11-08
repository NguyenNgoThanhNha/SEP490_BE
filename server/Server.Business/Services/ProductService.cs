using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Response;
using Server.Business.Models;
using Server.Business.Services;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}



