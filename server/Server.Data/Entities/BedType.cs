using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("BedType")]
public class BedType
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BedTypeId { get; set; }
    
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    public string? Thumbnail { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}