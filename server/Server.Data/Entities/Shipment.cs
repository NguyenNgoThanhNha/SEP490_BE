using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Shipment")]
public class Shipment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ShipmentId { get; set; }

    [ForeignKey("Order_Shipment")]
    public int OrderId { get; set; }
    public virtual Order Order { get; set; }

    public string TrackingNumber { get; set; } // Mã theo dõi đơn hàng
    public string ShippingCarrier { get; set; } // Đơn vị vận chuyển
    public decimal ShippingCost { get; set; } // Phí vận chuyển
    public DateTime? ShippedDate { get; set; } // Ngày giao hàng
    public DateTime? EstimatedDeliveryDate { get; set; } // Ngày dự kiến giao
    public string ShippingStatus { get; set; } = ShippingStatusEnum.Pending.ToString(); // Trạng thái vận chuyển
    
    public string RecipientName { get; set; } // Tên người nhận
    public string RecipientAddress { get; set; } // Địa chỉ người nhận
    public string RecipientPhone { get; set; } // Số điện thoại người nhận
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
