using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class UserRoutineStepRepository : GenericRepository<UserRoutineStep,int>
{
    public UserRoutineStepRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}