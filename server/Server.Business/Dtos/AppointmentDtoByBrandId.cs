using Server.Data;
using Server.Data.MongoDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class AppointmentDtoByBrandId
    {
        public int AppointmentId { get; set; }

        //public int CustomerId { get; set; }
       

        //public int StaffId { get; set; }
    

        public int ServiceId { get; set; }
      

        public int BranchId { get; set; }
        

        public DateTime AppointmentsTime { get; set; }

        public string Status { get; set; }

        public string Notes { get; set; }

        public string Feedback { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal SubTotal { get; set; } // (Quantity * UnitPrice)

        public string StatusPayment { get; set; } = OrderStatusPaymentEnum.Pending.ToString();

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public int? OrderId { get; set; }
        public DateTime AppointmentEndTime { get; set; }

        public UserDTO Customer { get; set; }
        public UserDTO Staff { get; set; }
        public ServiceDto Service { get; set; }

    }
}
