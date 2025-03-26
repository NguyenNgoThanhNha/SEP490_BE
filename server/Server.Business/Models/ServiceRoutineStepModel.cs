namespace Server.Business.Models;

public class ServiceRoutineStepModel
{
    public int Id { get; set; }
    
    public int StepId { get; set; }
    public virtual SkinCareRoutineStepModel Step { get; set; }
    
    public int ServiceId { get; set; }
    public virtual ServiceModel Service { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}