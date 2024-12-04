﻿using Server.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Response
{
    public class GetAllBlogResponse
    {
        public string message { get; set; }
        public List<BlogModel> data { get; set; }
        public Pagination pagination { get; set; }
    }
}
