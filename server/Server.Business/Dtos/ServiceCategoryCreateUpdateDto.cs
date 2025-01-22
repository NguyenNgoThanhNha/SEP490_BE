using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Server.Data;

namespace Server.Business.Dtos;

public class ServiceCategoryCreateUpdateDto
{
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    [ValidObjectStatus]
    public string? Status { get; set; }
    
    public IFormFile? Thumbnail { get; set; }
}


public class ValidObjectStatusAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; // Giá trị null hợp lệ nếu không bắt buộc
        }

        if (Enum.TryParse(typeof(ObjectStatus), value.ToString(), true, out var result) && result != null)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"The value '{value}' is not a valid status. Valid values are: {string.Join(", ", Enum.GetNames(typeof(ObjectStatus)))}.");
    }
}