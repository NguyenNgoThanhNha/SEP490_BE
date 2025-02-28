using Server.Business.Models;
using Server.Data.Entities;

namespace Server.Business.Commons.Response;

public class HistoryBookingResponse
{
    public string message { get; set; }
    public List<OrderModel> data { get; set; }
    public Pagination pagination {get;set;}
}
