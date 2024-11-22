namespace Server.Business.Models;

public class BranchModel
{
    public int BranchId { get; set; }
    
    public string BranchName { get; set; }
    
    public string BranchAddress { get; set; }
    
    public string BranchPhone { get; set; }
    
    public string? LongAddress { get; set; }
    
    public string? LatAddress { get; set; }
    
    public string? Status { get; set; }
    
    public int ManagerId { get; set; }
    public virtual UserModel ManagerBranch { get; set; }
    
    public int CompanyId { get; set; }
    /*public virtual Company Company { get; set; }*/
    
    /*public ICollection<Branch_Service> Branch_Services { get; set; }
    public ICollection<Branch_Product> Branch_Products { get; set; }*/
    
    public ICollection<BranchPromotionModel> Branch_Promotion { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}