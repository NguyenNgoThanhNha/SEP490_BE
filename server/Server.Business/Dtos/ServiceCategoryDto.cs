namespace Server.Business.Dtos;

public class ServiceCategoryDto
{
    public int ServiceCategoryId { get; set; }
    
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    public string? Status { get; set; }
    
    public string? Thumbnail { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}