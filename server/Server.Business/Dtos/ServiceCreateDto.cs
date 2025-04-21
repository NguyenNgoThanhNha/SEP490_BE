using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Server.Business.Dtos
{
    public class ServiceCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá dịch vụ phải lớn hơn 0")]
        public decimal Price { get; set; }
        public string Duration { get; set; }
    
        public string[]? Steps { get; set; }
        
        [Required(ErrorMessage = "Loại dịch vụ không được để trống")]
        public int ServiceCategoryId  { get; set; }
        public List<IFormFile>? images { get; set; }
    }
}
