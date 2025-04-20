using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class UpdateOrderDetailsStatusRequest
{
    [Required(ErrorMessage = "OrderDetailsIds is required")]
    public int[] OrderDetailsIds { get; set; } = Array.Empty<int>();
    
    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; }
}