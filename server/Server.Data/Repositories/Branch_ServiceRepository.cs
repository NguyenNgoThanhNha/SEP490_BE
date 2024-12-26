using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class Branch_ServiceRepository : GenericRepository<Branch_Service, int>
{
    public Branch_ServiceRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}