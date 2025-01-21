using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Service")]
public class Service
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ServiceId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Duration { get; set; }
    public string? Status { get; set; }
    
    public string? Steps { get; set; }
    
    [ForeignKey("Service_ServiceCategory")]
    public int ServiceCategoryId  { get; set; }
    public virtual ServiceCategory ServiceCategory { get; set; }
    
    public ICollection<Branch_Service> Branch_Services { get; set; }
    public ICollection<ServiceRoutine> ServiceRoutines { get; set; }

    public ICollection<ServiceImages> ServiceImages { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}