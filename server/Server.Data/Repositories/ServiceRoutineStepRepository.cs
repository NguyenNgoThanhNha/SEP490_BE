using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class ServiceRoutineStepRepository : GenericRepository<ServiceRoutineStep, int>
{
    public ServiceRoutineStepRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}