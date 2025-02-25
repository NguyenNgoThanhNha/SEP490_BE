using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class WorkScheduleRepository : GenericRepository<WorkSchedule, int>
{
    public WorkScheduleRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}