using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class CreateOrderWithDetailsRequest
    {
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "UserID phải tồn tại")]
        public int UserId { get; set; }
        
        public int? VoucherId { get; set; }
        //[Required]
        //[Range(1, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn 0")]
        //public decimal TotalAmount { get; set; }
        [Required]
        public string PaymentMethod { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Tiền ship phải lớn hơn 0")]
        public decimal ShippingCost { get; set; } 
        public DateTime? EstimatedDeliveryDate { get; set; } 
        public string? RecipientName { get; set; } 
        public string? RecipientAddress { get; set; } 
        public string? RecipientPhone { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 sản phẩm")]
        public List<OrderProductDto> Products { get; set; }
    }
}
