using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("UserRoutine")]
public class UserRoutine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserRoutineId { get; set; }
    
    [ForeignKey("UserRoutine")]
    public int UserId { get; set; }
    public virtual User User { get; set; }
    
    [ForeignKey("RoutineUser")]
    public int RoutineId { get; set; }
    public virtual SkincareRoutine Routine { get; set; }
    
    public string Status { get; set; }
    
    public string ProgressNotes { get; set; }
    
    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime EndDate { get; set; } = DateTime.Now;
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
    
    public ICollection<UserRoutineStep> UserRoutineSteps { get; set; }
}