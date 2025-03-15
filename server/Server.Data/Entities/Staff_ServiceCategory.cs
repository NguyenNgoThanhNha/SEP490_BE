using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Staff_ServiceCategory")]
public class Staff_ServiceCategory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Staff_ServiceCategoryId { get; set; } 
    
    [ForeignKey("StaffInfo_Category")]
    public int StaffId { get; set; }
    
    [ForeignKey("ServiceInfo_Category")]
    public int ServiceCategoryId { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    public virtual Staff StaffInfo { get; set; }
    public virtual ServiceCategory ServiceCategory { get; set; }
    
}