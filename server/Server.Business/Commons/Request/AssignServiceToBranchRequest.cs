using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Request
{
    public class AssignServiceToBranchRequest
    {
        [Required]
        public int BranchId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 service.")]
        public List<int> ServiceIds { get; set; }
    }

}
