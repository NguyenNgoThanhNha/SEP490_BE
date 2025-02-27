using Microsoft.EntityFrameworkCore;
using Server.Data.Base;
using Server.Data.Entities;
using System.Linq.Expressions;

namespace Server.Data.Repositories;

public class WorkScheduleRepository : GenericRepository<WorkSchedule, int>
{
    private readonly AppDbContext _dbContext;

    public WorkScheduleRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WorkSchedule>> GetAllAsync(Expression<Func<WorkSchedule, bool>> predicate)
    {
        return await _dbContext.WorkSchedule
            .Include(ws => ws.Staff) // Nếu có quan hệ với Staff
            .Include(ws => ws.Shift) // Nếu có quan hệ với Shift
            .Where(predicate)
            .ToListAsync();
    }



}