using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class BedAvailabilityRepository : GenericRepository<BedAvailability, int>
{
    public BedAvailabilityRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}