using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class OrderProductDto
    {
        public int ProductBranchId { get; set; }
        public int Quantity { get; set; }      
    }
}