using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class CreateAccountWithPhoneRequest
{
    [Required(ErrorMessage = "PhoneNumber is required")]
    public string PhoneNumber { get; set; }
    
    [Required(ErrorMessage = "UserName is required")]
    public string UserName { get; set; }
    
    public string? Password { get; set; }
    public string? Email { get; set; }
}