using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class ServiceUpdateDto
    {

        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Duration { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Order ID is required")]
        [Range(1, 20, ErrorMessage = "Category ID must be between 1 and 20.")]
        public int CategoryId { get; set; }
    }
}
