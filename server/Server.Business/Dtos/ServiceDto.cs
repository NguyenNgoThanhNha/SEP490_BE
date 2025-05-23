﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public int ServiceCategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Duration { get; set; }
        
        public string Status { get; set; }
        
        public string? Steps { get; set; }
        
        public string[] images { get; set; } 
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
