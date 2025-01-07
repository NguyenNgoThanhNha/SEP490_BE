using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class SkinHealthRepository : GenericRepository<SkinHealth, int>
{
    public SkinHealthRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}