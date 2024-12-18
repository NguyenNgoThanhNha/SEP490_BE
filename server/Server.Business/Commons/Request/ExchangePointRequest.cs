using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Request
{
    public class ExchangePointRequest
    {
        [Required]
        public int UserId { get; set; } // ID của khách hàng

        [Required]
        public int PromotionId { get; set; } // ID của Promotion muốn đổi
    }

}
