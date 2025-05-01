using Server.Data.Entities;

namespace Server.Business.Commons.Response;

public class GetAllNotificationsByUserIdResponse
{
    public string? Message { get; set; }
    public List<Notifications> Data { get; set; } = new List<Notifications>();
    public Pagination Pagination { get; set; } = new Pagination();
}