using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;

namespace Server.Data.Entities;
[Table("UserRoutineStep")]
public class UserRoutineStep
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserRoutineStepId { get; set; }
    
    [ForeignKey("UserRoutine")]
    public int UserRoutineId { get; set; }
    public virtual UserRoutine UserRoutine { get; set; }
    
    public string StepName { get; set; } // Tên bước (VD: Rửa mặt, Dưỡng ẩm)
    
    public string Description { get; set; } // Mô tả chi tiết bước này
    
    public int Step { get; set; } // Thứ tự thực hiện bước
    
    public DateTime StartDate { get; set; } // Ngày bắt đầu thực hiện bước
    public DateTime EndDate { get; set; } // Ngày kết thúc thực hiện bước
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}