using Server.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Models
{
    public class CategoryModel
    {
        public int CategoryId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? SkinTypeSuitable { get; set; }

        public string? Status { get; set; }

        public string? ImageUrl { get; set; }

        public List<ProductModel>? Products { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        
    }
}
