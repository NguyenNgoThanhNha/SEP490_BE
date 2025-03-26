using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("ServiceRoutineStep")]
public class ServiceRoutineStep
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("SkinCareRoutineStep")]
    public int StepId { get; set; }
    public virtual SkinCareRoutineStep Step { get; set; }

    [ForeignKey("Service")]
    public int ServiceId { get; set; }
    public virtual Service Service { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
