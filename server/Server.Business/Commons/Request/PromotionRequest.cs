using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class PromotionRequest
{
    [Required(ErrorMessage = "PromotionName is required")]
    public string PromotionName { get; set; }
    public string? PromotionDescription { get; set; }
    
    [Required(ErrorMessage = "DiscountPercent is required")]
    public decimal DiscountPercent { get; set; } 
    
    [Required(ErrorMessage = "StartDate is required")]
    public DateTime StartDate { get; set; }
    
    [Required(ErrorMessage = "EndDate is required")]
    public DateTime EndDate { get; set; }
    
    public string? Status { get; set; }
}