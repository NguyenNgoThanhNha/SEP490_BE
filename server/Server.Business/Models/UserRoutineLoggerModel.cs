namespace Server.Business.Models;

public class UserRoutineLoggerModel
{
    public int UserRoutineLoggerId { get; set; }
    
    public int StepId { get; set; }
    public virtual SkinCareRoutineStepModel UserRoutineStep { get; set; }
    
    public int? StaffId { get; set; } // Ai thực hiện bước này
    public virtual StaffModel? Staff { get; set; }
    
    public int? UserId { get; set; } // Ai thực hiện bước này
    public virtual UserInfoModel? User { get; set; }

    public DateTime ActionDate { get; set; } = DateTime.Now; // Thời gian thực hiện bước

    public string Status { get; set; } // Trạng thái: Đã hoàn thành, Bỏ qua, v.v.
    
    public string Step_Logger { get; set; } // Nội dung thực hiện bước

    public string Notes { get; set; } // Ghi chú nếu có

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}