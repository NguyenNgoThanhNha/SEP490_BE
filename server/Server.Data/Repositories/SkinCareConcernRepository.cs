using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class SkinCareConcernRepository : GenericRepository<SkincareRoutineConcern, int>
{
    public SkinCareConcernRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}