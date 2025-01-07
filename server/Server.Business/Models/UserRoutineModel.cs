namespace Server.Business.Models;

public class UserRoutineModel
{
    public int UserRoutineId { get; set; }
    
    public int UserId { get; set; }
    public virtual UserInfoModel User { get; set; }
    
    public int RoutineId { get; set; }
    public virtual SkincareRoutineModel Routine { get; set; }
    
    public string Status { get; set; }
    
    public string ProgressNotes { get; set; }
    
    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime EndDate { get; set; } = DateTime.Now;
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}