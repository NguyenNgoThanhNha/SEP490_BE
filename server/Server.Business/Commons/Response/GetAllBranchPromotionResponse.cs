using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class GetAllBranchPromotionResponse
{
    public string message { get; set; }
    public List<BranchPromotionModel> data { get; set; }
    public Pagination pagination {get;set;}
}