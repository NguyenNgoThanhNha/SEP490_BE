using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class YearlyBookingStatsDto
    {
        public int BranchId { get; set; }
        public int Year { get; set; }
        public List<MonthlyStatDto> MonthlyStats { get; set; }
    }
}
