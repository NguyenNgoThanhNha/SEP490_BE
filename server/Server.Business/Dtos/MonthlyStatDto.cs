using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class MonthlyStatDto
    {
        public int Month { get; set; }
        public int TotalBookings { get; set; }
        public int TotalServicesBooked { get; set; }
    }

}
