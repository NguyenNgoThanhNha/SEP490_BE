using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Models
{
    public class CustomerAppointmentModel: AppointmentsModel
    {
        public int? TotalSteps { get; set; }
    }
}
