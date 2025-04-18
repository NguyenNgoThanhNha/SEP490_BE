using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("ProductFeedback")]
public class ProductFeedback
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductFeedbackId { get; set; }
    
    [ForeignKey("Product_Feedback")]
    public int ProductId { get; set; }
    public virtual Product Product { get; set; }
    
    //[ForeignKey("Product_Feedback_Customer")]
    [ForeignKey("CustomerId")]
    public int? CustomerId { get; set; }
    public virtual User? Customer { get; set; } // ✅ Navigation property mới

    public int? UserId { get; set; }
    public virtual User? User { get; set; }

    
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