using System.ComponentModel.DataAnnotations;

namespace Server.Business.Models;

public class ShiftModel
{
    public int ShiftId { get; set; }
    
    public string ShiftName { get; set; } = string.Empty; // Tên ca (Sáng, Chiều, Tối)

    [Required]
    public TimeSpan StartTime { get; set; } // Giờ bắt đầu

    [Required]
    public TimeSpan EndTime { get; set; } // Giờ kết thúc
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}