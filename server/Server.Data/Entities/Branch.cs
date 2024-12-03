using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("Branch")]
public class Branch
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BranchId { get; set; }

    public string BranchName { get; set; }

    public string BranchAddress { get; set; }

    public string BranchPhone { get; set; }

    public string? LongAddress { get; set; }

    public string? LatAddress { get; set; }

    public string? Status { get; set; }

    [ForeignKey("ManagerBranch")]
    public int ManagerId { get; set; }
    public virtual User ManagerBranch { get; set; }

    [ForeignKey("Company")]
    public int CompanyId { get; set; }
    public virtual Company Company { get; set; }

    public ICollection<Branch_Service> Branch_Services { get; set; }
    public ICollection<Branch_Product> Branch_Products { get; set; }

    public ICollection<Branch_Promotion> Branch_Promotion { get; set; }
    public ICollection<Staff> Staffs { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}