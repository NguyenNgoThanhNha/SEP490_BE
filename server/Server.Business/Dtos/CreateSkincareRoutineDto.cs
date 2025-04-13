using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class CreateSkincareRoutineDto
    {
        [Required(ErrorMessage = "Tên gói liệu trình không được để trống")]
        public string? Name { get; set; }
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Số bước không được để trống")]
        [Range(1, 10, ErrorMessage = "Số bước phải nằm trong khoảng từ 1 đến 10")]
        public int TotalSteps { get; set; }
        
        [Required(ErrorMessage = "Loại da không được để trống")]
        public string[] TargetSkinTypes { get; set; }
        public decimal? TotalPrice { get; set; }
    }

}
