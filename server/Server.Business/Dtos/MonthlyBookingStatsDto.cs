using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class MonthlyBookingStatsDto
    {
        public class BookingStatsDto
        {
            public int BranchId { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }
            public int TotalBookings { get; set; }        // Số lượt đặt dịch vụ
            public int TotalServicesBooked { get; set; }  // Tổng quantity
        }

    }

}
