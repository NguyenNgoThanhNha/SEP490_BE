using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Server.Data;

namespace Server.Business.Commons.Request;

public class AppointmentFeedbackRequest
{
    public int AppointmentId { get; set; }
    
    public int? CustomerId { get; set; }
    
    public string? Comment { get; set; }
    
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int? Rating { get; set; }
    
    public IFormFile? ImageBefore { get; set; }
    
    public IFormFile? ImageAfter { get; set; }
}