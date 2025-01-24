namespace Server.Business.Commons.Request;

public class DepositRequest
{
    public int orderId { get; set; }
    
    public string totalAmount { get; set; }
    
    public string percent { get; set; }
    
    public  PayOsRequest Request { get; set; }
}