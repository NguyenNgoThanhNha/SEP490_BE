using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Company")]
public class Company
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CompanyId { get; set; } 

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }
    
    public string? Description { get; set; }

    [StringLength(15)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime UpdatedDate { get; set; } = DateTime.Now;
    
    public virtual ICollection<Branch> Branches { get; set; }
    public virtual ICollection<Product> Products { get; set; }
}