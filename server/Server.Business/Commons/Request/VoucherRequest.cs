namespace Server.Business.Commons.Request;

public class VoucherRequest
{
    public string Status { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}
