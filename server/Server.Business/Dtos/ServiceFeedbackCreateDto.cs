using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class ServiceFeedbackCreateDto
    {
        public int ServiceId { get; set; }
        public int UserId { get; set; }
        public int? CustomerId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? Status { get; set; }
        public string? ImageBefore { get; set; }
        public string? ImageAfter { get; set; }
        public string? CreatedBy { get; set; }
    }

}
