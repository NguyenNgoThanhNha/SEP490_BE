using System;

namespace Server.Business.Dtos
{
    public class ProductDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty; 

        public string ProductDescription { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? Volume { get; set; }
        public string? Dimension { get; set; }
        public int Quantity { get; set; }

        public decimal? Discount { get; set; }

        public int CategoryId { get; set; }

        public int CompanyId { get; set; }

        public string CategoryName { get; set; }

        public string CompanyName { get; set; } 

        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
