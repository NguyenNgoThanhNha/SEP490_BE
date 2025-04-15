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
        public List<SlotWorkingDto> SlotWorkings { get; set; } = new List<SlotWorkingDto>();
    }


}
