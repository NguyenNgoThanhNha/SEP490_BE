using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class RoomRepository : GenericRepository<Room, int>
{
    public RoomRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}