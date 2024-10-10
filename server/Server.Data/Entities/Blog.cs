using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Blog")]
public class Blog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BlogId { get; set; }
    
    public string Title { get; set; }
    
    public string Content { get; set; }
    
    [ForeignKey("AuthoBlog")]
    public int AuthorId { get; set; }
    public virtual User Author { get; set; }
    
    public string Status { get; set; }
    
    public string? Note { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}