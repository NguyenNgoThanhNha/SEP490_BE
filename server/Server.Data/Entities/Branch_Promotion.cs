using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("Branch_Promotion")]
public class Branch_Promotion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int PromotionId { get; set; }
    public  Promotion Promotion { get; set; }
    
    public int BranchId { get; set; }
    public  Branch Branch { get; set; }
    
    public string? Status { get; set; }
    
    public int? StockQuantity { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}