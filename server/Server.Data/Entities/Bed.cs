using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("Bed")]
public class Bed
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BedId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Thumbnail { get; set; }
    
    [ForeignKey("Room")]
    public int RoomId { get; set; }
    public virtual Room Room { get; set; }
    
    [ForeignKey("BedType")]
    public int BedTypeId { get; set; }
    public virtual BedType BedType { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}