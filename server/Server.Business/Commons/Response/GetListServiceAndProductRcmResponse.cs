using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class GetListServiceAndProductRcmResponse
{
    public UserInfoModel UserInfo { get; set; }
    public List<ServiceModel> Services { get; set; }
    public List<ProductModel> Products { get; set; }
}