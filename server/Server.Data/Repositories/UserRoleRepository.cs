using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class UserRoleRepository : GenericRepository<UserRole, int>
{
    public UserRoleRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}