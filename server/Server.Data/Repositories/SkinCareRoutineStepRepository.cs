using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class SkinCareRoutineStepRepository : GenericRepository<SkinCareRoutineStep, int>
{
    public SkinCareRoutineStepRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}