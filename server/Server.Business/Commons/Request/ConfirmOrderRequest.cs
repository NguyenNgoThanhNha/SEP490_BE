using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class ConfirmOrderRequest
{
    [Required(ErrorMessage = "Order Id is requrired")]
    public int orderId { get; set; }
    
    [Required(ErrorMessage = "Total Amount is requrired")]
    public string totalAmount { get; set; }
    
    public PayOsRequest Request { get; set; }
}