using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class AppointmentFeedbackCreateDto
    {
        public int AppointmentId { get; set; }
        public int CustomerId { get; set; }
        public int StaffId { get; set; }

        public string? Comment { get; set; }
        public int Rating { get; set; }

        public string? Status { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdateBy { get; set; }

        public string ImageBefore { get; set; } = null!;
        public string ImageAfter { get; set; } = null!;
    }


}
