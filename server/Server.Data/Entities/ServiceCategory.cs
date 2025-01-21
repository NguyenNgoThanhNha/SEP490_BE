using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("ServiceCategory")]
public class ServiceCategory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ServiceCategoryId { get; set; }
    
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    public string? Status { get; set; }
    
    public string? Thumbnail { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
    
    public virtual ICollection<Service> Services { get; set; }
}