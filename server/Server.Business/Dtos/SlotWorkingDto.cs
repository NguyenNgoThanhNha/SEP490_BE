﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class SlotWorkingDto
    {
        public int ScheduleId { get; set; }
        public int StaffId { get; set; }
        public int ShiftId { get; set; }
        public int DayOfWeek { get; set; }
        public DateTime WorkDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public ShiftDto Shift { get; set; }
    }

}
