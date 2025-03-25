using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class ProductDetailDto
    {
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
        public int CategoryId { get; set; }
        public int CompanyId { get; set; }
        public string? Dimension { get; set; }    
        public decimal? Volume { get; set; }
        public string? Status { get; set; }
        public string? Brand { get; set; }
        public string CategoryName { get; set; }
        public string CompanyName { get; set; }
        public string? SkinTypeSuitable { get; set; }
        public CategoryDetailDto Category { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string[] images { get; set; }
    }
}