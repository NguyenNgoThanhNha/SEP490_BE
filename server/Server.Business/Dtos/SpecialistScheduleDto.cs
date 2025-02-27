using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class SpecialistScheduleDto
    {
        public int StaffId { get; set; }
        public string FullName { get; set; }
        public List<WorkScheduleDto> Schedules { get; set; }
    }

    public class WorkScheduleDto
    {
        public int ScheduleId { get; set; }
        public DateTime WorkDate { get; set; }
        public int DayOfWeek { get; set; } // Thứ 2 - Thứ 7
        public string ShiftName { get; set; } // Ví dụ: "Ca Sáng"
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } // Trạng thái (Active, Inactive)
    }

}
