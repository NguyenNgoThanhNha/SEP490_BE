using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class ServiceCategoryRepository : GenericRepository<ServiceCategory, int>
{
    public ServiceCategoryRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}