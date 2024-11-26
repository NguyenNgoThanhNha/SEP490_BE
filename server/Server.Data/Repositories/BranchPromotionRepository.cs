using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class BranchPromotionRepository : GenericRepository<Branch_Promotion, int>
{
    public BranchPromotionRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}