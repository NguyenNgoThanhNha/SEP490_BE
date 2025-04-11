using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class CreateSkincareRoutineDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Steps { get; set; }
        public string? Frequency { get; set; }
        public string? TargetSkinTypes { get; set; }
        public decimal? TotalPrice { get; set; }
    }

}
