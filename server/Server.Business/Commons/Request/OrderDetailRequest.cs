using System.ComponentModel.DataAnnotations;
using Server.Data;

namespace Server.Business.Commons.Request;

public class OrderDetailRequest
{
    [Required(ErrorMessage = "OrderId is required!")]
    public int ProductId { get; set; }
    public int? PromotionId { get; set; }
    
    [Required(ErrorMessage = "Quantity is required!")]
    public int Quantity { get; set; }
    public string Status { get; set; } = OrderStatusEnum.Pending.ToString();
    public string StatusPayment { get; set; } = OrderStatusPaymentEnum.Pending.ToString();
}