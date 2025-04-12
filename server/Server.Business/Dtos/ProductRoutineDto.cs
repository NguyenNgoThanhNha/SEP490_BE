using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class ProductRoutineDto
    {
        public int ProductRoutineId { get; set; }
       // public int ProductId { get; set; }
        //public int RoutineId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public ProductDto Product { get; set; }
        public SkincareRoutineDto Routine { get; set; }
    }

}
