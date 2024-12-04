using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("ProductImages")]
public class ProductImages
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductImagesId { get; set; }
    
    [ForeignKey("Product_Images")]
    public int ProductId { get; set; }
    public virtual Product Product { get; set; }

    public string image { get; set; }
}