using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class ListStaffFreeInTimeResponse
{
     public string Message { get; set; }
     public List<StaffFreeInTimeResponse> Data { get; set; }
}

public class StaffFreeInTimeResponse
{
    public int ServiceId { get; set; }
    public DateTime StartTime { get; set; }
    public List<StaffModel> Staffs { get; set; }
}
