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
    
    [ForeignKey("SkinCareRoutineStep")]
    public int SkinCareRoutineStepId { get; set; } // Liên kết với bước gốc trong liệu trình
    public SkinCareRoutineStep SkinCareRoutineStep { get; set; }

    public string StepStatus { get; set; } = "Pending"; 
    
    public DateTime StartDate { get; set; } // Ngày bắt đầu thực hiện bước
    public DateTime EndDate { get; set; } // Ngày kết thúc thực hiện bước
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}