using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Models
{
    public class ProductFilterRequest
    {
        public int BrandId { get; set; }         // Lọc theo brand
        public string? Brand { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }      // Giá thấp nhất
        public decimal? MaxPrice { get; set; }      // Giá cao nhất
        public string? SortBy { get; set; }         // "price_asc" hoặc "price_desc"
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }

}
