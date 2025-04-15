using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class GetStaffLeaveOfBranchResponse
{
    public int BranchId { get; set; }
    
    public int Month { get; set; }
    
    public List<StaffLeaveModel> StaffLeaves { get; set; } = new List<StaffLeaveModel>();
}

public class GetStaffLeaveDetailResponse
{
    public StaffLeaveModel StaffLeave { get; set; }
    
    public List<AppointmentsModel> Appointments { get; set; } = new List<AppointmentsModel>();
}