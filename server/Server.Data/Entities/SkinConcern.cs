using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("SkinConcern")]
public class SkinConcern
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SkinConcernId { get; set; }
    
    [Required]
    public string Name { get; set; } // Ví dụ: "Da dầu", "Mụn trứng cá"

    [Required]
    public string Code { get; set; } // Ví dụ: "oily_skin", "acne"

    public ICollection<SkincareRoutineConcern> SkincareRoutineConcerns { get; set; }
}