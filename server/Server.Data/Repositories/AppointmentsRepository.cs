using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class AppointmentsRepository : GenericRepository<Appointments, int>
{
    public AppointmentsRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}