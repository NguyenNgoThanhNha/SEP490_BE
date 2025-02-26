using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class StaffScheduleDto
    {
        public int StaffId { get; set; }
        public string FullName { get; set; } // Tên nhân viên
        public List<WorkScheduleDto> Schedules { get; set; } = new List<WorkScheduleDto>();
    }

}
