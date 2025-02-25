namespace Server.Business.Commons.Request;

public class StaffLeaveRequest
{
    public int StaffId { get; set; }

    public DateTime LeaveDate { get; set; } // Ngày xin nghỉ

    public string Reason { get; set; } = string.Empty; // Lý do nghỉ
}