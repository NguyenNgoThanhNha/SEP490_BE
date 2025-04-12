using Server.Business.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Response
{
    public class GetAllBranchServicePaginationResponse
    {
        public string? message { get; set; }
        public List<BranchServiceDto> data { get; set; }
        public Pagination pagination { get; set; }
    }
}
