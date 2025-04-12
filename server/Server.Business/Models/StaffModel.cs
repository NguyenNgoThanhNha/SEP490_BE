using Server.Data.Entities;

namespace Server.Business.Models;

public class StaffModel
{
    public int StaffId { get; set; }
    public int UserId { get; set; }
    public int BranchId { get; set; }
    public int RoleId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    public virtual UserInfoModel StaffInfo { get; set; }

    // Thêm danh sách lịch làm việc
    //public virtual ICollection<WorkScheduleModel> WorkSchedules { get; set; }
}
