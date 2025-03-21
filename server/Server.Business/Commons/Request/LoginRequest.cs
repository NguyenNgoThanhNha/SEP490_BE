using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class LoginRequest
{
    [Required(ErrorMessage = "Email or phone number is required")]
    public string Identifier { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
}
