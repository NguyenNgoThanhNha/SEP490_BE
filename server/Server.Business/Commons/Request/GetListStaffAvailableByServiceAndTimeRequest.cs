namespace Server.Business.Commons.Request;

public class GetListStaffAvailableByServiceAndTimeRequest
{
    public int ServiceId { get; set; }
    public int BranchId { get; set; }
    public DateTime WorkDate { get; set; }
    
    public TimeSpan StartTime { get; set; }
}