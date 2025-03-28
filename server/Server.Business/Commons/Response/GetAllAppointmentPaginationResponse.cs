using Server.Business.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Response
{
    public class GetAllAppointmentPaginationResponse
    {
        public AppointmentDtoByBrandId data { get; set; }
        public string message { get; set; }
    }

}
