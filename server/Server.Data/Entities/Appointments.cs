﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Appointments")]
public class Appointments
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AppointmentId { get; set; }
    
    [ForeignKey("Order_Appointments")]
    public int? OrderId { get; set; }
    public virtual Order Order { get; set; }
    
    [ForeignKey("Customer_Appointments")]
    public int CustomerId { get; set; }
    public virtual User Customer { get; set; }
    
    [ForeignKey("Staff_Appointments")]
    public int StaffId { get; set; }
    public virtual Staff Staff { get; set; }
    
    [ForeignKey("Service_Appointments")]
    public int ServiceId { get; set; }
    public virtual Service Service { get; set; }
    
    [ForeignKey("Branch_Appointments")]
    public int BranchId { get; set; }
    public virtual Branch Branch { get; set; }
    
    [ForeignKey("Room_Appointments")]
    public int? RoomId { get; set; }
    public virtual Room Room { get; set; }
    
    
    [ForeignKey("Bed_Appointments")]
    public int? BedId { get; set; }
    public virtual Bed Bed { get; set; }
    
    public DateTime AppointmentsTime { get; set; }
    
    public string Status { get; set; }
    
    public string Notes { get; set; }
    
    public string Feedback { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal SubTotal  { get; set; } // (Quantity * UnitPrice)
    
    public string StatusPayment { get; set; } = OrderStatusPaymentEnum.Pending.ToString();
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}