using Server.Business.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Response
{
    public class ProductSoldResponse
    {
        public List<ProductSoldByBranchDto> Items { get; set; }
        public int TotalQuantitySold { get; set; }
    }
}
