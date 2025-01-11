namespace Server.Business.Dtos;

public class PromotionDTO
{
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
}