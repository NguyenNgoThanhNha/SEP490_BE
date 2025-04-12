using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("SkincareRoutineConcern")]
public class SkincareRoutineConcern
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int SkincareRoutineId { get; set; }
    public SkincareRoutine SkincareRoutine { get; set; }

    public int SkinConcernId { get; set; }
    public SkinConcern SkinConcern { get; set; }
}