namespace Server.Business.Commons.Request;

public class ConfirmDeleteRequest
{
    public string Email { get; set; }
    public string OTP { get; set; }
}