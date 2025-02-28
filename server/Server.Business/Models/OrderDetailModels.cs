using Server.Data;

namespace Server.Business.Models;

public class OrderDetailModels
{
    public int OrderDetailId { get; set; }
    
    public int? OrderId { get; set; }
    public virtual OrderModel Order { get; set; }
    
    public int? ProductId { get; set; }
    public virtual ProductModel Product { get; set; }
    
    public int? PromotionId { get; set; }
    public virtual PromotionModel? Promotion { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal SubTotal  { get; set; } // (Quantity * UnitPrice)
    
    public decimal? DiscountAmount { get; set; } // (SubTotal * DiscountPercent)
    public string Status { get; set; } = OrderStatusEnum.Pending.ToString();
    
    public string StatusPayment { get; set; } = OrderStatusPaymentEnum.Pending.ToString();
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}