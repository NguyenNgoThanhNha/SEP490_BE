namespace Server.Business.Models;

public class UserRoutineStepModel
{
    public int UserRoutineStepId { get; set; }
    
    public int UserRoutineId { get; set; }
    
    public int SkinCareRoutineStepId { get; set; } // Liên kết với bước gốc trong liệu trình
    public SkinCareRoutineStepModel SkinCareRoutineStep { get; set; }

    public string StepStatus { get; set; } = "Pending"; 
    
    public DateTime StartDate { get; set; } // Ngày bắt đầu thực hiện bước
    public DateTime EndDate { get; set; } // Ngày kết thúc thực hiện bước
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}