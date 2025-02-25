using Server.Data;

namespace Server.Business.Models;

public class StaffLeaveModel
{
    public int StaffLeaveId { get; set; }

    public int StaffId { get; set; }
    public StaffModel Staff { get; set; } = null!;

    public DateTime LeaveDate { get; set; } // Ngày xin nghỉ

    public string Reason { get; set; } = string.Empty; // Lý do nghỉ

    public string Status { get; set; } = StaffLeaveStatus.Pending.ToString(); // Mặc định là chờ duyệt

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}