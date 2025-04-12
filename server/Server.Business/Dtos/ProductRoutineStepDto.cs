using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    namespace Server.Business.Dtos
    {
        public class ProductRoutineStepDto
        {
            public int Id { get; set; }

            //public int StepId { get; set; }
            public SkinCareRoutineStepDto Step { get; set; }

            //public int ProductId { get; set; }
            public ProductDto Product { get; set; }

            public DateTime CreatedDate { get; set; }
            public DateTime UpdatedDate { get; set; }
        }
    }

}
