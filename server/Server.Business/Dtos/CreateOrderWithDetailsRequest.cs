using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class CreateOrderWithDetailsRequest
    {
        public int UserId { get; set; }
        public int? VoucherId { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public List<OrderProductDto> Products { get; set; }
    }
}
