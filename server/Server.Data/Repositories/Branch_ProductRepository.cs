using Server.Data.Base;
using Server.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data.Repositories
{
    public class Brand_ProductRepository : GenericRepository<Branch_Product, int>
    {
        public Brand_ProductRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
