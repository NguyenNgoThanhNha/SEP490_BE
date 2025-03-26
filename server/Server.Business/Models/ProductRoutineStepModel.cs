namespace Server.Business.Models;

public class ProductRoutineStepModel
{
    public int Id { get; set; }
    
    public int StepId { get; set; }
    public virtual SkinCareRoutineStepModel Step { get; set; }
    
    public int ProductId { get; set; }
    public virtual ProductModel Product { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}