using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("ServiceRoutine")]
public class ServiceRoutine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ServiceRoutineId { get; set; }
    
    [ForeignKey("ServiceRoutine")]
    public int ServiceId { get; set; }
    public virtual Service Service { get; set; }
    
    [ForeignKey("RoutineService")]
    public int RoutineId { get; set; }
    public virtual SkincareRoutine Routine { get; set; }
    
    public string Status { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}