using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class CreateBranchProductDto
    {
        public int ProductId { get; set; }
        public int BranchId { get; set; }
        public string? Status { get; set; }
        public int StockQuantity { get; set; }
    }
}
