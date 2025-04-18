using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class GetListShiftWithServiceOfStaffResponse
{
    public int ServiceId { get; set; }
    
    public List<StaffWithMultipleShift> WorkingStaffs { get; set; } = new List<StaffWithMultipleShift>();
}

public class StaffWithMultipleShift
{
    public StaffModel Staff { get; set; }
    public List<ShiftModel> Shifts { get; set; } = new List<ShiftModel>();
}