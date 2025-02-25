using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("AppointmentFeedback")]
public class AppointmentFeedback
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AppointmentFeedbackId { get; set; }
    
    [ForeignKey("Appointment_Feedback")]
    public int AppointmentId { get; set; }
    public virtual Appointments Appointment { get; set; }
    
    [ForeignKey("Appointment_Feedback_Customer")]
    public int? CustomerId { get; set; }
    public virtual User? User { get; set; }
    
    public string? Comment { get; set; }
    
    public int? Rating { get; set; }

    public string Status { get; set; } = FeedbackStatus.Pending.ToString();
    
    public string? CreatedBy { get; set; }
    
    public string? UpdatedBy { get; set; }
    
    public string? ImageBefore { get; set; }
    
    public string? ImageAfter { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}