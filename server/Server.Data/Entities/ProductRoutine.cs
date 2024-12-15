using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("ProductRoutine")]
public class ProductRoutine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductRoutineId { get; set; }
    
    [ForeignKey("ProductRoutine")]
    public int ProductId { get; set; }
    public virtual Product Products { get; set; }
    
    [ForeignKey("RoutineProduct")]
    public int RoutineId { get; set; }
    public virtual SkincareRoutine Routine { get; set; }
    
    public string Status { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}