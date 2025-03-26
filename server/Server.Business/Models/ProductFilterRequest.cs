using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Models
{
    public class ProductFilterRequest
    {
        public int? BrandId { get; set; }         // Lọc theo brand
        public decimal? MinPrice { get; set; }      // Giá thấp nhất
        public decimal? MaxPrice { get; set; }      // Giá cao nhất
        public string? SortBy { get; set; }         // "price_asc" hoặc "price_desc"
    }

}
