using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Notifications")]
public class Notifications
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int NotificationId { get; set; }
    
    [ForeignKey("User_Notifications")]
    public int CustomerId { get; set; }
    public virtual User User { get; set; }
    
    public string Content { get; set; }
    
    public string Type { get; set; } // loại thông báo (lịch hẹn, khuyến mãi,...)
    
    public bool? isRead { get; set; }
    
    public int? ObjectId { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}