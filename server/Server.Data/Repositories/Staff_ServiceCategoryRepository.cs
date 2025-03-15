using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class Staff_ServiceCategoryRepository : GenericRepository<Staff_ServiceCategory, int>
{
    public Staff_ServiceCategoryRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}