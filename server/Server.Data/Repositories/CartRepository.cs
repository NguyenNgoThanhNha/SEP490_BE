using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories
{
    public class CartRepository : GenericRepository<Cart, int>
    {
        public CartRepository(AppDbContext dbContext) : base(dbContext)
        {
        }


    }
}
