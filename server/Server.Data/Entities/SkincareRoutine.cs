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
    public string? Steps { get; set; }
    public string? Frequency { get; set; }
    public string? TargetSkinTypes { get; set; }
    public decimal? TotalPrice { get; set; }
    public ICollection<UserRoutine> UserRoutines { get; set; }
    public ICollection<ProductRoutine> ProductRoutines { get; set; }
    public ICollection<ServiceRoutine> ServiceRoutines { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}