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
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải lớn hơn hoặc bằng 0")]
        public int Quantity { get; set; }
        
        public string? Brand { get; set; }
        public int CategoryId { get; set; }
        
        public int CompanyId { get; set; }
        public IFormFile? Image { get; set; }

    }
}


