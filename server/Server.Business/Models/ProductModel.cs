using Server.Business.Dtos;
using Server.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public decimal Price { get; set; }

        public decimal? Volume { get; set; }

        public string? Dimension { get; set; }

        public int Quantity { get; set; }

        public decimal? Discount { get; set; }

        public string? Status { get; set; }
        public string? SkinTypeSuitable { get; set; }

        public int CompanyId { get; set; }

        public string CompanyName { get; set; } // Thêm CompanyName
        
        public int CategoryId  { get; set; }
        
        public CategoryModel Category { get; set; } // Bao gồm chi tiết Category
        public string[] images { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public BranchDTO Branch { get; set; }
        public PromotionDTO? Promotion { get; set; }

    }

}
