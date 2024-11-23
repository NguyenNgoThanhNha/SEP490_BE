using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class BranchRepository : GenericRepository<Branch, int>
{
    public BranchRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}