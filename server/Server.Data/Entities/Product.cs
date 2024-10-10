using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Product")]
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductId { get; set; }
    
    public string ProductName { get; set; }
    
    public string ProductDescription { get; set; }
    
    public decimal Price { get; set; }
    
    public int Quantity { get; set; }
    
    [ForeignKey("Product_Category")]
    public int CategoryId { get; set; }
    
    public decimal? Discount { get; set; }
    
    [ForeignKey("Product_Branch")]
    public int BranchId { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
    
    public virtual Category Category { get; set; }
    public virtual Branch Branch { get; set; }
}