﻿namespace Server.Business.Dtos;

public class BranchDTO
{
    public int BranchId { get; set; }
    
    public string BranchName { get; set; }
    
    public string BranchAddress { get; set; }
    
    public string BranchPhone { get; set; }
    
    public string? LongAddress { get; set; }
    
    public string? LatAddress { get; set; }
    
    public string? Status { get; set; }
    
    public int ManagerId { get; set; }
    public virtual UserDTO ManagerBranch { get; set; }

    public int? District { get; set; }

    public int? WardCode { get; set; }

    public int CompanyId { get; set; }   
    
    public ICollection<BranchPromotionDTO> Branch_Promotion { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}