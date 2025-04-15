namespace Server.Business.Commons.Request;

public class GetAllOrderRequest
{
    public string? OrderType { get; set; }
    public string? OrderStatus { get; set; }
    public string? PaymentStatus { get; set; }
    public int? BranchId { get; set; }
    public int? PageSize { get; set; } = 10;
    public int? PageIndex { get; set; } = 1;
}