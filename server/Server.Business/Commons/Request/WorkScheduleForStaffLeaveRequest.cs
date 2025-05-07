using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class WorkScheduleForStaffLeaveRequest
{
    [Required(ErrorMessage = "StaffLeaveId is required")]
    public int StaffLeaveId { get; set; }
    
    [Required(ErrorMessage = "StaffReplaceId is required")]
    public int StaffReplaceId { get; set; }
    
    [Required(ErrorMessage = "ShiftId is required")]
    public int ShiftId { get; set; }
    
    [Required(ErrorMessage = "WorkDate is required")]
    public DateTime WorkDate { get; set; }
}