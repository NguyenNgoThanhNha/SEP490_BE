using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Promotion")]
public class Promotion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PromotionId { get; set; }

    public string PromotionName { get; set; }

    public string? PromotionDescription { get; set; }

    public decimal DiscountPercent { get; set; } 
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
    
    public string? Status { get; set; }
    
    public string? Image { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now; 

    public DateTime UpdatedDate { get; set; } = DateTime.Now;
    
    public ICollection<Branch_Product> Branch_Promotions { get; set; }
}
