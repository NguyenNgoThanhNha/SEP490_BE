namespace Server.Business.Dtos;

public class BranchPromotionDTO
{
    public int Id { get; set; }
    
    public int PromotionId { get; set; }
    public  PromotionDTO Promotion { get; set; }
    
    public int BranchId { get; set; }
    public  BranchDTO Branch { get; set; }
    
    public string? Status { get; set; }
    
    //public int? StockQuantity { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}