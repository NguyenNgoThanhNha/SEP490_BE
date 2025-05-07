namespace Server.Business.Models;

public class UserRoutineLoggerModel
{
    public int UserRoutineLoggerId { get; set; }
    
    public int StepId { get; set; }
    public virtual UserRoutineStepModel UserRoutineStep { get; set; }
    
    public int? ManagerId { get; set; } // Ai thực hiện bước này
    public virtual UserInfoModel? Manager { get; set; }
    
    public int? UserId { get; set; } // Ai thực hiện bước này
    public virtual UserInfoModel? User { get; set; }

    public DateTime ActionDate { get; set; } = DateTime.Now; // Thời gian thực hiện bước

    public string Status { get; set; } // Trạng thái: Đã hoàn thành, Bỏ qua, v.v.
    
    public string Step_Logger { get; set; } // Nội dung thực hiện bước

    public string Notes { get; set; } // Ghi chú nếu có

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}