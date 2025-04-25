using Server.Data;
using Server.Data.Entities;

namespace Server.Business.Dtos;

public class AppointmentsDTO
{
    public int AppointmentId { get; set; }

    public int CustomerId { get; set; }
    public virtual UserDTO Customer { get; set; }

    public int StaffId { get; set; }
    public virtual StaffDto Staff { get; set; }

    public int ServiceId { get; set; }
    public virtual ServiceDto Service { get; set; }

    public int BranchId { get; set; }
    public virtual BranchDTO Branch { get; set; }

    public DateTime AppointmentsTime { get; set; }
    public DateTime AppointmentEndTime { get; set; }

    public string Status { get; set; }

    public string Notes { get; set; }

    public string Feedback { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal SubTotal { get; set; } // (Quantity * UnitPrice)

    public string StatusPayment { get; set; } = OrderStatusPaymentEnum.Pending.ToString();

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
    public int? TotalSteps { get; set; }
}