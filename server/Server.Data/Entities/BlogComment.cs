using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("BlogComment")]
public class BlogComment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BlogCommentId { get; set; }
    
    [ForeignKey("Blog_BlogComment")]
    public int BlogId { get; set; }
    public virtual Blog Blog { get; set; }
    
    [ForeignKey("Customer_BlogComment")]
    public int CustomerId { get; set; }
    public virtual User Customer { get; set; }
    
    public string Comment { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}