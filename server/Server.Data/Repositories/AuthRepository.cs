using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class AuthRepository : GenericRepository<User, int>
{
    public AuthRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}