namespace Server.Business.Commons.Request;

public class WorkSheduleRequest
{
    public int StaffId { get; set; }
    
    public int ShiftId { get; set; }
    
    public DateTime FromDate { get; set; }
    
    public DateTime ToDate { get; set; }
}