using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Request
{
    public class GetServiceByRoutineRequest
    {
        public int RoutineId { get; set; }
        public int UserId { get; set; }
    }

}
