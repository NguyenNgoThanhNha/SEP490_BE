using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data;

public class SkinHealthImageRepository : GenericRepository<SkinHealthImage, int>
{
    public SkinHealthImageRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}