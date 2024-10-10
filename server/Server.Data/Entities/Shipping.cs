using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Shipping")]
public class Shipping
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ShippingId { get; set; }
    
    public string ShippingAddress { get; set; }
    
    public string ShippingMethod { get; set; }
    
    public decimal ShippingCost { get; set; }
    
    public DateTime? ShippingDate { get; set; }
    
    public DateTime? DeliveryDate { get; set; }
    
    public string Status { get; set; }
    
    public string? TrackingNumber { get; set; }
    
    [ForeignKey("Shipping_Order")]
    public int OrderId { get; set; }
    public virtual Order Order { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}