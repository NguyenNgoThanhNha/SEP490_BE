using Server.Data.Entities;

namespace Server.Business.Models;

public class BranchPromotionModel
{
    public int Id { get; set; }
    
    public int PromotionId { get; set; }
    public  PromotionModel Promotion { get; set; }
    
    public int BranchId { get; set; }
    public  BranchModel Branch { get; set; }
    
    public string? Status { get; set; }
    
    public int? StockQuantity { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}