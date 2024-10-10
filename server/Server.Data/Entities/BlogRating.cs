using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("BlogRating")]
public class BlogRating
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BlogRatingId { get; set; }
    [ForeignKey("Blog_BlogRating")]
    public int BlogId { get; set; }
    public virtual Blog Blog { get; set; }
    
    [ForeignKey("Customer_BlogRating")]
    public int CustomerId { get; set; }
    public virtual User Customer { get; set; }
    
    public int Rate { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}