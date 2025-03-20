using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories
{
    public class ProductCartRepository : GenericRepository<ProductCart, int>
    {
        public ProductCartRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
