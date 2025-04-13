using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class CreateSkinCareRoutineStepDto
    {
        [Required(ErrorMessage = "SkincareRoutineId là bắt buộc")]
        public int SkincareRoutineId { get; set; }

        [Required(ErrorMessage = "Tên bước (Name) là bắt buộc")]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Range(1, 10, ErrorMessage = "Step phải nằm trong khoảng từ 1 đến 10")]
        public int Step { get; set; }

        public int? IntervalBeforeNextStep { get; set; }
        
        public List<int> ProductIds { get; set; } = new List<int>();
        public List<int> ServiceIds { get; set; } = new List<int>();
    }  

}
