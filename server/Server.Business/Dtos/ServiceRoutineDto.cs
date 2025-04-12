using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class ServiceRoutineDto
    {
        public int ServiceRoutineId { get; set; }       
        public string Status { get; set; }

        public ServiceDto Service { get; set; }         // Dịch vụ
        public SkincareRoutineDto Routine { get; set; } // Routine
    }

}
