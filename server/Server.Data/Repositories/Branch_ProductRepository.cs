using Microsoft.EntityFrameworkCore;
using Server.Data.Base;
using Server.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data.Repositories
{
    public class Branch_ProductRepository : GenericRepository<Branch_Product, int>
    {
        private readonly AppDbContext _context;
        public Branch_ProductRepository(AppDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
        public async Task<Branch_Product?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Branch_Products
                .Include(x => x.Product)
                .Include(x => x.Branch)
                .Include(x => x.Promotion)
                .FirstOrDefaultAsync(x => x.Id == id);
        }


    }
}
