using Microsoft.IdentityModel.Tokens;

namespace Server.Business.Commons.Response;

public class LoginResult
{
    public bool Authenticated { get; set; }
    public SecurityToken? Token { get; set; }
    public SecurityToken? Refresh { get; set; }
}