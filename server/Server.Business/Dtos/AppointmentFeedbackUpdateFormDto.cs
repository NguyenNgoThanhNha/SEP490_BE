﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class AppointmentFeedbackUpdateFormDto
    {
        //public string? Comment { get; set; }
        //public int? Rating { get; set; }
        //public string? Status { get; set; }

        //public IFormFile? ImageBefore { get; set; }
        public IFormFile? ImageAfter { get; set; }
    }

}
