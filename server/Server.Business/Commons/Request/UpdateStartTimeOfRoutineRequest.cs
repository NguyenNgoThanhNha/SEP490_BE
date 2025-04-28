using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class UpdateStartTimeOfRoutineRequest
{
    [Required(ErrorMessage = "OrderId là bắt buộc!")]
    public int OrderId { get; set; }
    
    [Required(ErrorMessage = "Bước bắt đầu cập nhật là bắt buộc!")]
    public int FromStep { get; set; }
    
    [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc!")]
    public DateTime StartTime { get; set; }
}