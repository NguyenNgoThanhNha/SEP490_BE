using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class PromotionRepository: GenericRepository<Promotion, int>
{
    public PromotionRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}