namespace Server.Business.Models;

public class ServiceRoutineModel
{
    public int ServiceRoutineId { get; set; }
    
    public int ServiceId { get; set; }
    public virtual ServiceModel Service { get; set; }
    
    public int RoutineId { get; set; }
    public virtual SkincareRoutineModel Routine { get; set; }
    
    public string Status { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}