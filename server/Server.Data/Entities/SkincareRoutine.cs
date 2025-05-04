using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("SkincareRoutine")]
public class SkincareRoutine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SkincareRoutineId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? TotalSteps { get; set; }
    public string? TargetSkinTypes { get; set; }
    public decimal? TotalPrice { get; set; }
    
    public string? Status { get; set; }
    public ICollection<UserRoutine> UserRoutines { get; set; }
    public ICollection<ProductRoutine> ProductRoutines { get; set; }
    public ICollection<ServiceRoutine> ServiceRoutines { get; set; }
    public ICollection<SkinCareRoutineStep> SkinCareRoutineSteps { get; set; }
    
    public ICollection<SkincareRoutineConcern> SkincareRoutineConcerns { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}