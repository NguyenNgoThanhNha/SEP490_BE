using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("UserRoutineLogger")]
public class UserRoutineLogger
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserRoutineLoggerId { get; set; }
    
    [ForeignKey("UserRoutineStep")]
    public int StepId { get; set; }
    public virtual UserRoutineStep UserRoutineStep { get; set; }
    
    [ForeignKey("UserRoutine_Staff")]
    public int? StaffId { get; set; } // Ai thực hiện bước này
    public virtual Staff? Staff { get; set; }
    
    [ForeignKey("UserRoutine_Customer")]
    public int? UserId { get; set; } // Ai thực hiện bước này
    public virtual User? User { get; set; }

    public DateTime ActionDate { get; set; } = DateTime.Now; // Thời gian thực hiện bước

    public string Status { get; set; } // Trạng thái: Đã hoàn thành, Bỏ qua, v.v.
    
    public string Step_Logger { get; set; } // Nội dung thực hiện bước

    public string Notes { get; set; } // Ghi chú nếu có

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}