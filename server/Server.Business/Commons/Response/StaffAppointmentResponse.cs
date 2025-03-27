using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class StaffAppointmentResponse
{
    public int StaffId { get; set; }
    public StaffModel Staff { get; set; }
    public List<AppointmentsInfoModel> Appointments { get; set; }
}