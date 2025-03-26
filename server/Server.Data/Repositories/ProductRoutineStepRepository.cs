using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class ProductRoutineStepRepository : GenericRepository<ProductRoutineStep, int>
{
    public ProductRoutineStepRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}