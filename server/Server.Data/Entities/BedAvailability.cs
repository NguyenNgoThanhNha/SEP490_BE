using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("BedAvailability")]
public class BedAvailability
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BedAvailabilityId { get; set; }
    
    [ForeignKey("Bed_BedAvailability")]
    public int? BedId { get; set; }
    public virtual Bed Bed { get; set; }

    [ForeignKey("Room_BedAvailability")]
    public int? RoomId { get; set; }
    public virtual Room Room { get; set; }
    
    public string Status { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}