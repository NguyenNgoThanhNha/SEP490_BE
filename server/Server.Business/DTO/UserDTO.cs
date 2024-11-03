namespace Server.Business.DTO;

public class UserDTO
{
    public int UserId { get; set; }
    
    public string? UserName { get; set; }
    
    public string? FullName { get; set; }
    
    public string Email { get; set; }

    public string? Avatar { get; set; }
    
    public string? Gender { get; set; }
    
    public string? City { get; set; }
    
    public string? Address { get; set; }

    public DateTime? BirthDate { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string? CreateBy { get; set; }

    public DateTimeOffset? CreateDate { get; set; }

    public string? ModifyBy { get; set; }

    public DateTimeOffset? ModifyDate { get; set; }

    public string? Status { get; set; }

    public int BonusPoint { get; set; } = 0;
    
    public string TypeLogin { get; set; }
    
    public int RoleID { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}