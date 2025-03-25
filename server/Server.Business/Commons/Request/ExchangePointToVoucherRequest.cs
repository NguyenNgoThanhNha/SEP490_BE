namespace Server.Business.Commons.Request;

public class ExchangePointToVoucherRequest
{
    public int UserPoint { get; set; }
    public int UserId { get; set; }
    public int VoucherId { get; set; }
}