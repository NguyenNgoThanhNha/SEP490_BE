namespace Server.Business.Commons.Request;

public class WebhookRequest
{
    public string code { get; set; }
    public string desc { get; set; }
    public bool success { get; set; }
    public DataObject data { get; set; }
    public string signature { get; set; }
}

public class DataObject
{
    public int orderCode { get; set; }
    public int amount { get; set; }
    public string description { get; set; }
    public string accountNumber { get; set; }
    public string reference { get; set; }
    public string transactionDateTime { get; set; }
    public string currency { get; set; }
    public string paymentLinkId { get; set; }
    public string code { get; set; }
    public string desc { get; set; }
    public string? counterAccountBankId { get; set; }
    public string? counterAccountBankName { get; set; }
    public string? counterAccountName { get; set; }
    public string? counterAccountNumber { get; set; }
    public string? virtualAccountName { get; set; }
    public string? virtualAccountNumber { get; set; }
}
