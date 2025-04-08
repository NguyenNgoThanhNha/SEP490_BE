using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class CreateBranchServiceDto
    {
        public int BranchId { get; set; }
        public int ServiceId { get; set; }
        public string? Status { get; set; }
    }

}
