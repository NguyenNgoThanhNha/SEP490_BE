using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class SkincareRoutineRepository : GenericRepository<SkincareRoutine, int>
{
    public SkincareRoutineRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}