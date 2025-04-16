using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class GetListOrderFilterResponse
{
    public string message { get; set; }
    public List<OrderModel> data { get; set; }
    public Pagination pagination { get; set; }
}