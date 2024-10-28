using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class FirstStepResquest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;

    public string? UserName { get; set; }

    public string Password { get; set; }

    public string? FullName { get; set; }

    [Phone(ErrorMessage = "Invalid phone")]
    public string? Phone { get; set; } = null!;

    public string? City { get; set; }

    public string? Address { get; set; }

    public string? Link { get; set; }

    public string TypeAccount { get; set; }
}