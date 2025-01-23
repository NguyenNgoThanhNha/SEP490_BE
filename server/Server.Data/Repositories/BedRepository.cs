using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class BedRepository: GenericRepository<Bed, int>
{
    public BedRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
