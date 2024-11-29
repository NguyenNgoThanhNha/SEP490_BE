using Server.Data.Entities;

namespace Server.Business.Dtos;

public class AppointmentsDTO
{
    public int AppointmentsId { get; set; }
    
    public int CustomerId { get; set; }
    public virtual UserDTO Customer { get; set; }
    
    public int StaffId { get; set; }
    public virtual StaffDTO Staff { get; set; }
    
    public int ServiceId { get; set; }
    public virtual ServiceDto Service { get; set; }
    
    public int BranchId { get; set; }
    public virtual BranchDTO Branch { get; set; }
    
    public DateTime AppointmentsTime { get; set; }
    
    public string Status { get; set; }
    
    public string Notes { get; set; }
    
    public string Feedback { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}