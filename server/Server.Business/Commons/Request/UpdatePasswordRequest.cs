namespace Server.Business.Commons.Request;

public class UpdatePasswordRequest
{
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}