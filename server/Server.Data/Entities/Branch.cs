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
    
    [ForeignKey("ManagerBranch")]
    public int ManagerId { get; set; }
    public virtual User ManagerBranch { get; set; }
    
    public ICollection<Branch_Service> Branch_Services { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}