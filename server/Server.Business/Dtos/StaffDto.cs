﻿namespace Server.Business.Dtos;

public class StaffDto
{
    public int StaffId { get; set; }
    public int? BranchId { get; set; }
    
    public int RoleId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    public virtual UserDTO StaffInfo { get; set; }
}