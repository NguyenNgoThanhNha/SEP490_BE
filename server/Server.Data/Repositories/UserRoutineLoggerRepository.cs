using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class UserRoutineLoggerRepository : GenericRepository<UserRoutineLogger, int>
{
    public UserRoutineLoggerRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}