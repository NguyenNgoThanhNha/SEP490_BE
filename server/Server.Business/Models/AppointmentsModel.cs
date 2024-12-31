using Server.Data.Entities;

namespace Server.Business.Models;

public class AppointmentsModel
{
    public int AppointmentId { get; set; }
    
    public int CustomerId { get; set; }
    public virtual UserModel Customer { get; set; }
    
    public int StaffId { get; set; }
    public virtual StaffModel Staff { get; set; }
    
    public int ServiceId { get; set; }
    public virtual ServiceModel Service { get; set; }
    
    public int BranchId { get; set; }
    public virtual BranchModel Branch { get; set; }
    
    public DateTime AppointmentsTime { get; set; }
    
    public string Status { get; set; }
    
    public string Notes { get; set; }
    
    public string Feedback { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}