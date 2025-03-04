using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("Staff")]
public class Staff
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int StaffId { get; set; }
    
    [ForeignKey("StaffInfo")]
    public int UserId { get; set; }

    [ForeignKey("Branch")]
    public int BranchId { get; set; }
    
    [ForeignKey("StaffRole")]
    public int RoleId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    public virtual User StaffInfo { get; set; }
    public virtual Branch Branch { get; set; }
    
    public virtual StaffRole Role { get; set; }
    public virtual ICollection<WorkSchedule> WorkSchedules { get; set; }

}