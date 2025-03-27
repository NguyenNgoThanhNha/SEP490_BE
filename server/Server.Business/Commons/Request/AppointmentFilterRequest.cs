using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Request
{
    public class AppointmentFilterRequest
    {
        public int BranchId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
