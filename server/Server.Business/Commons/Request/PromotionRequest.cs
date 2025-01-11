using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Server.Data;

namespace Server.Business.Commons.Request;

public class PromotionRequest : IValidatableObject
{
    [Required(ErrorMessage = "PromotionName is required")]
    public string PromotionName { get; set; }
    
    public string? PromotionDescription { get; set; }
    
    [Required(ErrorMessage = "DiscountPercent is required")]
    [Range(0, 100, ErrorMessage = "DiscountPercent must be between 0 and 100")]
    public decimal DiscountPercent { get; set; }
    
    [Required(ErrorMessage = "StartDate is required")]
    public DateTime StartDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "EndDate is required")]
    public DateTime EndDate { get; set; } = DateTime.Now;
    
    public string? Status { get; set; }
    
    public IFormFile? Image { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate StartDate and EndDate
        if (StartDate > EndDate)
        {
            yield return new ValidationResult("StartDate must be before EndDate", new[] { nameof(StartDate), nameof(EndDate) });
        }
        
        // Validate Status
        if (!string.IsNullOrEmpty(Status) && !Enum.TryParse(typeof(ObjectStatus), Status, true, out _))
        {
            yield return new ValidationResult($"Status must be one of the following values: {string.Join(", ", Enum.GetNames(typeof(ObjectStatus)))}", new[] { nameof(Status) });
        }
    }
}