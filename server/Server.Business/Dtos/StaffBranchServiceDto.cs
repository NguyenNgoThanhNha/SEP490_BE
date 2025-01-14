using Server.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class StaffBranchServiceDto
    {
        public Staff Staff { get; set; }
        //public Branch Branch { get; set; }
        public Server.Data.Entities.Service Service { get; set; }
    }
}
