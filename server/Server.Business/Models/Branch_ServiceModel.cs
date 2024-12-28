namespace Server.Business.Models;

public class Branch_ServiceModel
{
    public int Id { get; set; }
    
    public int BranchId { get; set; }
    public BranchModel Branch { get; set; }
    
    public string? Status { get; set; }
    
    public int ServiceId { get; set; }
    public ServiceModel Service { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}