using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class SkinConcernRepository : GenericRepository<SkinConcern, int>
{
    public SkinConcernRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}