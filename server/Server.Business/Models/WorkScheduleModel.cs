using System.ComponentModel.DataAnnotations;
using Server.Data.Entities;

namespace Server.Business.Models;
public class WorkScheduleModel
{
    public int Id { get; set; }
    
    public int StaffId { get; set; }
    
    public StaffModel Staff { get; set; } = null!;
    
    public int ShiftId { get; set; }
    public ShiftModel Shift { get; set; } = null!;
    
    [Range(1, 6)] // 1=Thứ 2, 6=Thứ 7
    public int DayOfWeek { get; set; } 
    
    public DateTime WorkDate { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}