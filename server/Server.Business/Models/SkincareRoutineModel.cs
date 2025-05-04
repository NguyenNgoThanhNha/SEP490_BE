namespace Server.Business.Models;

public class SkincareRoutineModel
{
    public int SkincareRoutineId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? TotalSteps { get; set; }
    public string? TargetSkinTypes { get; set; }
    
    public string? Status { get; set; }
    public decimal? TotalPrice { get; set; }
    public ICollection<UserRoutineModel> UserRoutines { get; set; }
    public ICollection<ProductRoutineModel> ProductRoutines { get; set; }
    public ICollection<ServiceRoutineModel> ServiceRoutines { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}