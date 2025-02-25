using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class StaffLeaveRepository : GenericRepository<StaffLeave, int>
{
    public StaffLeaveRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}