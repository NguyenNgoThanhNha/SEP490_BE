using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class UpdateOrderStatusSimpleRequest
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
    }

}
