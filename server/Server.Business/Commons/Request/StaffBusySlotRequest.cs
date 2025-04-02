using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Request
{
    public class StaffBusySlotRequest
    {
        public List<int> StaffIds { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

}
