namespace Server.Business.Models;

public class SkinCareRoutineStepModel
{
    public int SkinCareRoutineStepId { get; set; }
    
    public int SkincareRoutineId { get; set; }
    public SkincareRoutineModel SkincareRoutine { get; set; }
    
    public string Name { get; set; } // Tên bước
    
    public string? Description { get; set; } // Mô tả bước
    
    public int Step { get; set; } // Thứ tự thực hiện bước
    
    public TimeSpan? IntervalBeforeNextStep { get; set; } // Khoảng thời gian chờ trước bước tiếp theo
    
    public ICollection<ServiceRoutineStepModel> ServiceRoutineSteps { get; set; } = new List<ServiceRoutineStepModel>();
    public ICollection<ProductRoutineStepModel> ProductRoutineSteps { get; set; } = new List<ProductRoutineStepModel>();
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}