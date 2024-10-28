namespace Server.Business.Commons.Request;

public class LoginWithGGRequest
{
    public string UserName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Avatar { get; set; }
    public string TypeAccount { get; set; }
}