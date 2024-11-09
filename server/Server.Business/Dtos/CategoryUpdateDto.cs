using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class CategoryUpdateDto
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? SkinTypeSuitable { get; set; }

        public string Status { get; set; } = "true";

        public string? ImageUrl { get; set; }
    }
}
