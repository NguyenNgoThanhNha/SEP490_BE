using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Order")]
public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderId { get; set; }

    public int OrderCode { get; set; }

    [ForeignKey("Customer_Order")]
    public int CustomerId { get; set; }
    public virtual User Customer { get; set; }

    [ForeignKey("Voucher_Order")]
    public int? VoucherId { get; set; }
    public virtual Voucher? Voucher { get; set; }
    
    [ForeignKey("Routine_Order")]
    public int? RoutineId { get; set; }
    public virtual SkincareRoutine? Routine { get; set; }

    public decimal TotalAmount { get; set; }
    
    public decimal? DiscountAmount { get; set; }

    public string OrderType { get; set; } 
    public string Status { get; set; } = OrderStatusEnum.Pending.ToString();

    public string StatusPayment { get; set; } = OrderStatusPaymentEnum.Pending.ToString();

    public string? PaymentMethod { get; set; } = PaymentMethodEnum.PayOS.ToString();
    public string? Note { get; set; }
    
    public int? UserRoutineId { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    public virtual ICollection<Appointments> Appointments { get; set; }
    public virtual Shipment Shipment { get; set; }
    //public virtual Promotion Promotion { get; set; }    
}