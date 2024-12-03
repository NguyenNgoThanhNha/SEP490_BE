using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class StaffRepository : GenericRepository<Staff, int>
{
    public StaffRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}