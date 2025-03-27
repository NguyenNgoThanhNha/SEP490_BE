using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("ProductCart")]
public class ProductCart
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductCartId { get; set; }
    
    [ForeignKey("User_Cart_Product")]
    public int ProductBranchId { get; set; }
    public virtual Branch_Product ProductBranch { get; set; }
    
    [ForeignKey("User_Cart_Cart")]
    public int CartId { get; set; }
    public virtual Cart Cart { get; set; }
    
    public int Quantity { get; set; } = 1;
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now; 

}