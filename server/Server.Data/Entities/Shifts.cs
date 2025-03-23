namespace Server.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Shifts")]
public class Shifts
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ShiftId { get; set; }

    [Required]
    [StringLength(50)]
    public string ShiftName { get; set; } = string.Empty; // Tên ca (Sáng, Chiều, Tối)

    [Required]
    public TimeSpan StartTime { get; set; } // Giờ bắt đầu

    [Required]
    public TimeSpan EndTime { get; set; } // Giờ kết thúc
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    /*public ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();*/
}
