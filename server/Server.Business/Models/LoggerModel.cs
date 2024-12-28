namespace Server.Business.Models;

public class LoggerModel
{
    public int LoggerId { get; set; }
    
    public string Message { get; set; }
    
    public string Exception { get; set; }
    
    public string Status { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}