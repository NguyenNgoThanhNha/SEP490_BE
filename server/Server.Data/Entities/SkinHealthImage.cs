using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("SkinHealthImage")]
public class SkinHealthImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SkinHealthImageId { get; set; }
    
    public string ImageUrl { get; set; }
    
    public int SkinHealthId { get; set; }
    
    [ForeignKey("SkinHealthId")]
    public SkinHealth SkinHealth { get; set; } = null!;
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}