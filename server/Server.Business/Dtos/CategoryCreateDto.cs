﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class CategoryCreateDto
    {
       
        public string? Name { get; set; }

        public string? Description { get; set; }
        public string? ImageUrl { get; set; }       
    }
}
