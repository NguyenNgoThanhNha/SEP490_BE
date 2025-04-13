using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("SkinCareRoutineStep")]
public class SkinCareRoutineStep
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SkinCareRoutineStepId { get; set; }
    
    [Required]
    public int SkincareRoutineId { get; set; }
    public SkincareRoutine SkincareRoutine { get; set; }
    
    [Required]
    public string Name { get; set; } // Tên bước
    
    public string? Description { get; set; } // Mô tả bước
    
    public int Step { get; set; } // Thứ tự thực hiện bước
    
    public int? IntervalBeforeNextStep { get; set; } // Khoảng thời gian chờ trước bước tiếp theo
    
    public ICollection<ServiceRoutineStep> ServiceRoutineSteps { get; set; } = new List<ServiceRoutineStep>();
    public ICollection<ProductRoutineStep> ProductRoutineSteps { get; set; } = new List<ProductRoutineStep>();
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}