using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class UpdatePaymentMethodOrNoteRequest
    {
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; }  // Có thể null nếu không thay đổi phương thức thanh toán
        public string Note { get; set; }  // Có thể null nếu không thay đổi ghi chú       
    }

}
