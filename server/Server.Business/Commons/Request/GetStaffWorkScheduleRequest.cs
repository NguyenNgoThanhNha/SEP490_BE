using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class GetStaffWorkScheduleRequest
{
    [Required(ErrorMessage = "Staff Id này không được để trống")]
    public int[] StaffIds { get; set; }
    
    [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
    public DateTime WorkDate { get; set; }
}