using Server.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaService = Server.Data.Entities.Service;


namespace Server.Business.Dtos
{
    public class StaffBranchServiceDto
    {
        public Staff Staff { get; set; }
        //public Branch Branch { get; set; }
        public SpaService Service { get; set; }
    }
}
