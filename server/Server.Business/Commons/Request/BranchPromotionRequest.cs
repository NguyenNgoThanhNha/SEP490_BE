using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class BranchPromotionRequest
{
    [Required(ErrorMessage = "PromotionId is required!")]
    public int PromotionId { get; set; }
    
    [Required(ErrorMessage = "BranchId is required!")]
    public int BranchId { get; set; }
    
    public string? Status { get; set; }

    [Required(ErrorMessage = "StockQuantity is required!")]
    public int? StockQuantity { get; set; } = 1;
}