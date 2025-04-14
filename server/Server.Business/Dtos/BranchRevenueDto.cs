using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class BranchRevenueDto
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public decimal TotalRevenue { get; set; }
    }

}
