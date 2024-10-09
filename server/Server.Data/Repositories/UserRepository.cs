using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories
{
    public class UserRepository : GenericRepository<User, int>
    {
        public UserRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
