using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class ServiceRoutineStepDto
    {
        public int Id { get; set; }

        //public int ServiceId { get; set; }
        //public int StepId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public ServiceDto Service { get; set; }
        public SkinCareRoutineStepDto Step { get; set; }
    }
}
