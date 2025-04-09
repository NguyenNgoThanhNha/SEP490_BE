using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("ServiceFeedback")]
public class ServiceFeedback
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ServiceFeedbackId { get; set; }
    
    [ForeignKey("Service_Feedback")]
    public int ServiceId { get; set; }
    public virtual Service Service { get; set; }
    
    [ForeignKey("ServiceFeedback_Customer")]
    public int? CustomerId { get; set; }
    public virtual User? User { get; set; }
    public int? UserId { get; set; }

    public string? Comment { get; set; }
    
    public int? Rating { get; set; }

    public string Status { get; set; } = FeedbackStatus.NotFeedbacked.ToString();
    
    public string? CreatedBy { get; set; }
    
    public string? UpdatedBy { get; set; }
    
    public string? ImageBefore { get; set; }
    
    public string? ImageAfter { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}