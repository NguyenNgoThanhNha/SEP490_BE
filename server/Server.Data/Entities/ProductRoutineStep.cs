using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("ProductRoutineStep")]
public class ProductRoutineStep
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("SkinCareRoutineStep")]
    public int StepId { get; set; }
    public virtual SkinCareRoutineStep Step { get; set; }

    [ForeignKey("Product")]
    public int ProductId { get; set; }
    public virtual Product Product { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
