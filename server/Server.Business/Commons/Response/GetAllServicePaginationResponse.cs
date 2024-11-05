using Server.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Response
{
    public class GetAllServicePaginationResponse
    {
        public List<ServiceModel> data {get;set;}
        public Pagination pagination {get;set;}

    }

    public class Pagination
    {
        public int page { get; set; }
        public int totalPage { get; set; }
        public int totalCount { get; set; }
    }
}
