using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class AssignServiceToRoutineDto
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int RoutineId { get; set; }

      
    }

}
