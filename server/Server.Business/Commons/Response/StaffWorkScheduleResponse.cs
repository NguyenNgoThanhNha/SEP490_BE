using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class StaffWorkScheduleResponse
{
    public int StaffId { get; set; }
    
    public List<WorkScheduleModel> WorkSchedules { get; set; } = new List<WorkScheduleModel>();
}