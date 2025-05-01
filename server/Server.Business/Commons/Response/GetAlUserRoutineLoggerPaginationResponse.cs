using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class GetAlUserRoutineLoggerPaginationResponse
{
    public string? message { get; set; }
    public List<UserRoutineLoggerModel> data { get; set; }
    public Pagination pagination { get; set; }
}