using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class AppointmentFeedbackUpdateDto
    {
        public string? Comment { get; set; }
        public int Rating { get; set; }
        public string? Status { get; set; }
        public string? UpdatedBy { get; set; }
        public string? ImageBefore { get; set; }
        public string? ImageAfter { get; set; }
    }

}
