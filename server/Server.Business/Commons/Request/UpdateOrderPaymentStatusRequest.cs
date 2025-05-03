using Server.Data;

namespace Server.Business.Commons.Request;

public class UpdateOrderPaymentStatusRequest
{
    public int OrderId { get; set; }
    public string Status { get; set; }
}