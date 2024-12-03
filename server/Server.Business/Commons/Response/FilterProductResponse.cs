using Server.Business.Dtos;

namespace Server.Business.Commons.Response;

public class FilterProductResponse
{
    public List<ProductDto> data { get; set; }
}