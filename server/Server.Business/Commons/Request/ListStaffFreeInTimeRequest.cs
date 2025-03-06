using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class ListStaffFreeInTimeRequest
{
    [Required(ErrorMessage = "BranchId is required")]
    public int BranchId { get; set; }
    
    [Required(ErrorMessage = "ServiceId is required")]
    public int ServiceId { get; set; }
    
    public DateTime StartTime { get; set; }
}