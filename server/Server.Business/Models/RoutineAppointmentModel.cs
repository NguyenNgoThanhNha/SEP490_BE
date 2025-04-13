using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Models
{
    public class RoutineAppointmentModel
    {
        public int OrderId { get; set; }
        public int OrderCode { get; set; }
        public DateTime OrderDate { get; set; }

        public string Status { get; set; }
        public string StatusPayment { get; set; }

        public decimal TotalAmount { get; set; }

        public List<AppointmentsModel> Appointments { get; set; }

        public SkincareRoutineModel Routine { get; set; }
    }

}
