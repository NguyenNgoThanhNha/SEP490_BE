using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class GetAllAppointmentResponse
{
    public string message { get; set; }
    public List<AppointmentsModel> data { get; set; }
    public Pagination pagination {get;set;}
}