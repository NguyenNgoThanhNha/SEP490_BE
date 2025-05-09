﻿using Server.Data;
using Server.Data.Entities;

namespace Server.Business.Models;

public class AppointmentsModel
{
    public int AppointmentId { get; set; }
    
    public int? OrderId { get; set; }
    public virtual OrderModel Order { get; set; }
    
    public int CustomerId { get; set; }
    public virtual UserInfoModel Customer { get; set; }
    
    public int StaffId { get; set; }
    public virtual StaffModel Staff { get; set; }
    
    public int ServiceId { get; set; }
    public virtual ServiceModel Service { get; set; }
    
    public int BranchId { get; set; }
    public virtual BranchModel Branch { get; set; }
    
    public DateTime AppointmentsTime { get; set; }
    
    public DateTime AppointmentEndTime { get; set; }
    
    public string Status { get; set; } = OrderStatusEnum.Pending.ToString();
    
    public string Notes { get; set; }
    
    public string Feedback { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal SubTotal  { get; set; } // (Quantity * UnitPrice)
    
    public int? Step { get; set; }
    
    public string StatusPayment { get; set; } = OrderStatusPaymentEnum.Pending.ToString();
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
    public int? TotalSteps { get; set; }
}

public class AppointmentsInfoModel
{
    public int AppointmentId { get; set; }
    
    public int? OrderId { get; set; }
    public virtual OrderInfoModel Order { get; set; }
    
    public int CustomerId { get; set; }
    public virtual UserInfoModel Customer { get; set; }
    
    public int StaffId { get; set; }
    public virtual StaffModel Staff { get; set; }
    
    public int ServiceId { get; set; }
    public virtual ServiceModel Service { get; set; }
    
    public int BranchId { get; set; }
    public virtual BranchModel Branch { get; set; }
    
    public DateTime AppointmentsTime { get; set; }
    
    public DateTime AppointmentEndTime { get; set; }
    
    public string Status { get; set; } = OrderStatusEnum.Pending.ToString();
    
    public string Notes { get; set; }
    
    public string Feedback { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal SubTotal  { get; set; } // (Quantity * UnitPrice)
    
    public int? Step { get; set; }
    
    public string StatusPayment { get; set; } = OrderStatusPaymentEnum.Pending.ToString();
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
    public int? TotalSteps { get; set; }
}