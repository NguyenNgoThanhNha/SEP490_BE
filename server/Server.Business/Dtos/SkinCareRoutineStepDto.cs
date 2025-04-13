using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Business.Dtos.Server.Business.Dtos;
using Server.Business.Models;

namespace Server.Business.Dtos
{
    public class SkinCareRoutineStepDto : CreateSkinCareRoutineStepDto
    {
        public int SkinCareRoutineStepId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
    
    public class SkinCareRoutineStepDetailDto
    {
        public int SkinCareRoutineStepId { get; set; }
        
        public int SkincareRoutineId { get; set; }
        public SkincareRoutineDto SkincareRoutine { get; set; }
        
        public string Name { get; set; } // Tên bước
    
        public string? Description { get; set; } // Mô tả bước
    
        public int Step { get; set; } // Thứ tự thực hiện bước
    
        public int? IntervalBeforeNextStep { get; set; } // Khoảng thời gian chờ trước bước tiếp theo
    
        public ICollection<ServiceModel>? Services { get; set; } = new List<ServiceModel>();
        public ICollection<ProductModel> Products { get; set; } = new List<ProductModel>();
    
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }

}
