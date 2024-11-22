using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class GetAllPromotionResponse
{
    public string message { get; set; }
    public List<PromotionModel> data { get; set; }
    public Pagination pagination {get;set;}
}