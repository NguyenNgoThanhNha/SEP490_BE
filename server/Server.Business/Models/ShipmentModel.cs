using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Models
{
    public class ShipmentModel
    {
        public int ShipmentId { get; set; }
        public int OrderId { get; set; }
        public string TrackingNumber { get; set; }
        public string ShippingCarrier { get; set; }
        public decimal? ShippingCost { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string ShippingStatus { get; set; }
        public string RecipientName { get; set; }
        public string RecipientAddress { get; set; }
        public string RecipientPhone { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

}
