using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Server.Business.Commons.Request;

public class UserInfoUpdateRequest
{
    public int UserId { get; set; }

    [MaxLength(255)]
    [Required(ErrorMessage = "UserName is required")]
    public string? UserName { get; set; }
    
    [MaxLength(50)]
    public string? FullName { get; set; }

    [MaxLength(255)]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }

    public IFormFile? Avatar { get; set; }

    [MaxLength(100)]
    public string? Gender { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    public DateTime? BirthDate { get; set; }

    [MaxLength(15)]
    [Phone(ErrorMessage = "Invalid Phone Number")]
    public string? PhoneNumber { get; set; }
    
    public string? STK { get; set; }
    public string? Bank { get; set; }
    public string? QRCode { get; set; }
    
    public int? District { get; set; }
    
    public int? WardCode { get; set; }
}