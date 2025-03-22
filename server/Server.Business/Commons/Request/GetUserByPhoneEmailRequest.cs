namespace Server.Business.Commons.Request;

public class GetUserByPhoneEmailRequest
{
    public string? Phone { get; set; }
    public string? Email { get; set; }
}