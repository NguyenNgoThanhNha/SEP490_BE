using Server.Data;

namespace Server.Business.Models;

public class RoomModel
{
    public int RoomId { get; set; }
    
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    public string? Thumbnail { get; set; }

    public string? Status { get; set; } = ObjectStatus.Active.ToString();
    
    public int BranchId { get; set; }
    public virtual BranchModel Branch { get; set; }
    
    public int ServiceCategoryId { get; set; }
    public virtual ServiceCategoryModel ServiceCategory { get; set; }
    
    public List<BedModel>? Beds { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}