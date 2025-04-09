using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class StaffBusyTimeDto
    {
        public int StaffId { get; set; }
        public List<BusyTimeDto> BusyTimes { get; set; } = new();
    }

}
