using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class ServiceImageRepository : GenericRepository<ServiceImages, int>
{
    public ServiceImageRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}