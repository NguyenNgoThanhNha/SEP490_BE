using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Request
{
    public class AssignProductToBranchRequest
    {
        public int ProductId { get; set; }
        public int BranchId { get; set; }
        public int StockQuantity { get; set; }
    }

}
