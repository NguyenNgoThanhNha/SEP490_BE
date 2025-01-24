namespace Server.Business.Models;

public class BedTypeModel
{
    public int BedTypeId { get; set; }
    
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    public string? Thumbnail { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}