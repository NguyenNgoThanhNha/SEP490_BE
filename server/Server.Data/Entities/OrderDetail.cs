using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("OrderDetail")]
public class OrderDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderDetailId { get; set; }
    
    [ForeignKey("Order_OrderDetail")]
    public int? OrderId { get; set; }
    public virtual Order Order { get; set; }
    
    [ForeignKey("Product_OrderDetail")]
    public int? ProductId { get; set; }
    public virtual Product Product { get; set; }
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal SubTotal  { get; set; } // (Quantity * UnitPrice)
    public string Status { get; set; } = OrderStatusEnum.Pending.ToString();
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}