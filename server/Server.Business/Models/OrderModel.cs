using Server.Data;
using Server.Data.Entities;

namespace Server.Business.Models;

public class OrderModel
{
    public int OrderId { get; set; }

    public int OrderCode { get; set; }
    
    public int CustomerId { get; set; }
    public virtual UserInfoModel Customer { get; set; }
    
    public int VoucherId { get; set; }
    public virtual Voucher Voucher { get; set; }

    public decimal TotalAmount { get; set; }

    public string OrderType { get; set; } 
    
    public string Status { get; set; }
    
    public string Notes { get; set; }
    
    public string Feedback { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal SubTotal  { get; set; } // (Quantity * UnitPrice)
    
    public string StatusPayment { get; set; } = OrderStatusPaymentEnum.Pending.ToString();

    public string? Note { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
}