using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class UpdateStaffDto
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public int BranchId { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow; // Tự động cập nhật ngày
    }
}
