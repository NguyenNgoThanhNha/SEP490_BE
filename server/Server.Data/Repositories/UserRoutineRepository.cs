using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class UserRoutineRepository : GenericRepository<UserRoutine, int>
{
    public UserRoutineRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}