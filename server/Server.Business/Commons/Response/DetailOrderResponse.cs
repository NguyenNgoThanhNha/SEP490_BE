using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class DetailOrderResponse
{
    public string message { get; set; }
    public OrderModel data { get; set; }
}


