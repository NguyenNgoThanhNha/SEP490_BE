namespace Server.Business.Models;

public class StaffModel
{
    public int StaffId { get; set; }

    public int UserId { get; set; }
    
    public int BranchId { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    public virtual UserModel StaffInfo { get; set; }
}