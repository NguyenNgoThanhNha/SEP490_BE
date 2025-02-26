using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class CashierScheduleDto
    {
        public int StaffId { get; set; }
        public string FullName { get; set; } // Tên nhân viên cashier
        public List<WorkScheduleDto> Schedules { get; set; } = new List<WorkScheduleDto>();
    }

}
