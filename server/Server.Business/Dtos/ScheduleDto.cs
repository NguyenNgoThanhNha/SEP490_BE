using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class ScheduleDto
    {
        public int ScheduleId { get; set; }
        public int StaffId { get; set; }
        public string StaffName { get; set; } // Nếu cần thêm tên staff
        public int BranchId { get; set; }
        public string BranchName { get; set; } // Nếu cần thêm tên chi nhánh
        public string ShiftName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime WorkDate { get; set; }
    }
}
