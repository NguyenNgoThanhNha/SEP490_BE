namespace Server.Business.Models;

public class ProductRoutineModel
{
    public int ProductRoutineId { get; set; }
    
    public int ProductId { get; set; }
    public virtual ProductModel Products { get; set; }
    
    public int RoutineId { get; set; }
    public virtual SkincareRoutineModel Routine { get; set; }
    
    public string Status { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}