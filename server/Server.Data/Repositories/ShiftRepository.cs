using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class ShiftRepository : GenericRepository<Shifts, int>
{
    public ShiftRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}