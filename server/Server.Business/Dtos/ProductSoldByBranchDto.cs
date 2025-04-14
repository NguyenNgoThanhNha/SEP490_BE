using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class ProductSoldByBranchDto
    {
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalQuantitySold { get; set; }
    }
}
