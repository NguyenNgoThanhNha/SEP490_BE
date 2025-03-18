using Microsoft.EntityFrameworkCore;
using Server.Data.Base;
using Server.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data.Repositories
{
    public class ScheduleRepository : GenericRepository<Schedule, int>
    {
        private readonly AppDbContext _dbContext;
        public ScheduleRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<Schedule>> GetStaffScheduleAsync(int staffId, int year, int month)
        {
            return await _dbContext.Schedules
                .Where(s => s.StaffId == staffId &&
                            s.WorkDate.Year == year &&
                            s.WorkDate.Month == month)
                .ToListAsync();
        }
    }
}
