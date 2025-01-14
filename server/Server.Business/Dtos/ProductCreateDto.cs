using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class ProductCreateDto 
    {
        

        public string ProductName { get; set; }

        public string ProductDescription { get; set; }
        public string Dimension { get; set; }
        public decimal Volume { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public string? SkinTypeSuitable { get; set; }
        public decimal? Discount { get; set; }

        public int CategoryId { get; set; }
        public int CompanyId { get; set; }
        public IFormFile? Image { get; set; }

    }
}


