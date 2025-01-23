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
    
    public int? RoomId { get; set; }
    public virtual Room Room { get; set; }
    
    public int? BedId { get; set; }
    public virtual Bed Bed { get; set; }
    
    public DateTime AppointmentsTime { get; set; }
    
    public string Status { get; set; }
    
    public string Notes { get; set; }
    
    public string Feedback { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal SubTotal  { get; set; } // (Quantity * UnitPrice)
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}