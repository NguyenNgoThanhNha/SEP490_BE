using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class BranchProductDto
    {
        public int Id { get; set; }
       // public int ProductId { get; set; }
        public ProductDto Product { get; set; }
       // public int BranchId { get; set; }
        public BranchDTO Branch { get; set; }
        //public int? PromotionId { get; set; }
        public PromotionDTO? Promotion { get; set; }
        public string? Status { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        
    }
}
