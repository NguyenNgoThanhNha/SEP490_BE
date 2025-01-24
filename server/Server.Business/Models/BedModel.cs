using Server.Data;

namespace Server.Business.Models;

public class BedModel
{
    public int BedId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Thumbnail { get; set; }
    
    public int RoomId { get; set; }
    public virtual RoomModel Room { get; set; }
    
    public int BedTypeId { get; set; }
    public virtual BedTypeModel BedType { get; set; }
    
    public string? Status { get; set; } = ObjectStatus.Active.ToString();
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}