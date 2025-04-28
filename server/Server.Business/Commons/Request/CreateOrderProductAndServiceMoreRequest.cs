using System.ComponentModel.DataAnnotations;
using Server.Data;

namespace Server.Business.Commons.Request;

public class CreateOrderProductAndServiceMoreRequest
{
    public int[] ProductIds { get; set; }
    public int[] Quantity { get; set; }
    public int[] ServiceIds { get; set; }
    
    public string Status { get; set; } = OrderStatusEnum.Pending.ToString();
    public string StatusPayment { get; set; } = OrderStatusPaymentEnum.Pending.ToString();
    
    [Required(ErrorMessage = "BranchId là bắt buộc!")]
    public int BranchId { get; set; }
}