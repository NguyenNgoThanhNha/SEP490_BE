﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Server.Business.Dtos
{
    public class ServiceCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Duration { get; set; }
        
        public List<IFormFile>? images { get; set; }
    }
}
