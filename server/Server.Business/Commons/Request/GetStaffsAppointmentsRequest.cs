namespace Server.Business.Commons.Request;

public class GetStaffsAppointmentsRequest
{
    public List<int> StaffIds { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}