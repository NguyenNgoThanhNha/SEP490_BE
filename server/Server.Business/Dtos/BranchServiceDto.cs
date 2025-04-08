using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class BranchServiceDto
    {
        public int Id { get; set; }
       // public int BranchId { get; set; }
        public BranchDTO Branch { get; set; }

      //  public int ServiceId { get; set; }
        public ServiceDto Service { get; set; }

        public string? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

}
