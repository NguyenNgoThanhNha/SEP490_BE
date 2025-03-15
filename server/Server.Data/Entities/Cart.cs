using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("Cart")]
public class Cart
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CartId { get; set; }
    
    [ForeignKey("Customer_Cart")]
    public int CustomerId { get; set; }
    public virtual User Customer { get; set; }
    
    public decimal TotalPrice { get; set; } = 0;
    public ICollection<ProductCart> ProductCarts { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}