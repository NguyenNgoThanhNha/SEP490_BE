using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class GetListShiftWithServiceOfStaffRequest
{
    public int[] ServiceIds { get; set; } = Array.Empty<int>();
    [Required(ErrorMessage = "BranchId là bắt buộc")]
    public int BranchId { get; set; }
    [Required(ErrorMessage = "Ngày làm việc là bắt buộc")]
    public DateTime WorkDate { get; set; }
}