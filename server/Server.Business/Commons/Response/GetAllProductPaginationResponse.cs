﻿using Server.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Response
{
    public class GetAllProductPaginationResponse
    {
        public string message { get; set; }
        public List<ProductModel> data {get;set;}
        public Pagination pagination {get;set;}

    }  


}
