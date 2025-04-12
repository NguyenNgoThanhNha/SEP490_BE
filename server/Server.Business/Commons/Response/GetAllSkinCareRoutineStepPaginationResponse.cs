using Server.Business.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Response
{
    public class GetAllSkinCareRoutineStepPaginationResponse
    {
        public string message { get; set; }
        public List<SkinCareRoutineStepDto> data { get; set; }
        public Pagination pagination { get; set; }
    }

}
