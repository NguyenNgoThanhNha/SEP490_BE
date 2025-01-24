namespace Server.Business.Commons;

public class PayOsRequest
{
    public string returnUrl { get; set; } = "home";
    public string cancelUrl { get; set; } = "home";
}