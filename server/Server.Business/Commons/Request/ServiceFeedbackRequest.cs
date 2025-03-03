using System.ComponentModel.DataAnnotations;
using Server.Data;

namespace Server.Business.Commons.Request;

public class ServiceFeedbackRequest
{
    public int ServiceId { get; set; }
    
    public int? CustomerId { get; set; }
    
    public string? Comment { get; set; }
    
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int? Rating { get; set; }
    public string? ImageBefore { get; set; }
    
    public string? ImageAfter { get; set; }
}