using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Data.Entities;

[Table("Work_Schedule")]
public class WorkSchedule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int StaffId { get; set; }
    [ForeignKey("StaffId")]
    public Staff Staff { get; set; } = null!;
    
    public int ShiftId { get; set; }
    [ForeignKey("ShiftId")]
    public Shifts Shift { get; set; } = null!;

    [Required]
    [Range(1, 6)] // 1=Thứ 2, 6=Thứ 7
    public int DayOfWeek { get; set; } 
    
    public DateTime WorkDate { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}