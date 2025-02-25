using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("StaffLeave")]
public class StaffLeave
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int StaffLeaveId { get; set; }

    [ForeignKey("StaffLeave")]
    public int StaffId { get; set; }
    public virtual Staff Staff { get; set; }

    public DateTime LeaveDate { get; set; } // Ngày xin nghỉ

    public string Reason { get; set; } = string.Empty; // Lý do nghỉ

    public string Status { get; set; } = StaffLeaveStatus.Pending.ToString(); // Mặc định là chờ duyệt

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}