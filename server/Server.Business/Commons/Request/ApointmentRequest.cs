using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class ApointmentRequest
{
    [Required(ErrorMessage = "StaffId is required!")]
    public int[] StaffId { get; set; }
    
    [Required(ErrorMessage = "ServiceId is required!")]
    public int[] ServiceId { get; set; }
    
    [Required(ErrorMessage = "BranchId is required!")]
    public int BranchId { get; set; }
    
    public DateTime AppointmentsTime { get; set; }
    
    public string? Status { get; set; }
    
    public string Notes { get; set; }
    
    public string? Feedback { get; set; }
    
    public int VoucherId { get; set; }
}