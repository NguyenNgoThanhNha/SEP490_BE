using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class ProductDetailInCartDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal? Discount { get; set; }
        public int CategoryId { get; set; }
        public int CompanyId { get; set; }
        public string Dimension { get; set; }
        public decimal? Volume { get; set; }
        public string Status { get; set; }
        public string Brand { get; set; }
        public string SkinTypeSuitable { get; set; }
        public CategoryDetailInCartDto Category { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<string> Images { get; set; } = new();
        public int? BrandId { get; set; }
        public int ProductBranchId { get; set; }
        public int StockQuantity { get; set; }


    }

}
