using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class GetAllServiceCategoryResponse
{
    public string message { get; set; }
    public List<ServiceCategoryModel> data {get;set;}
    public Pagination pagination {get;set;}

}