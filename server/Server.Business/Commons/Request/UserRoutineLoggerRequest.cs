using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class UserRoutineLoggerRequest
{
    [Required(ErrorMessage = "StepId là bat buộc")]
    public int StepId { get; set; }
    
    public int? ManagerId { get; set; } // Ai thực hiện bước này
    
    public int? UserId { get; set; } // Ai thực hiện bZước này

    public DateTime ActionDate { get; set; } = DateTime.Now; // Thời gian thực hiện bước
    
    public string Step_Logger { get; set; } // Nội dung thực hiện bước

    public string Notes { get; set; } // Ghi chú nếu có
}