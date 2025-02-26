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
    
    [Required(ErrorMessage = "DayOfWeek is required")]
    [Range(1, 6)] // 1=Thứ 2, 6=Thứ 7
    public int DayOfWeek { get; set; } 
}