using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Room")]
public class Room
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RoomId { get; set; }
    
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    public string? Thumbnail { get; set; }

    public string? Status { get; set; } = ObjectStatus.Active.ToString();
    
    [ForeignKey("Branch")]
    public int BranchId { get; set; }
    public virtual Branch Branch { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}