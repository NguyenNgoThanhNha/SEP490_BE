using Server.Business.Dtos;

namespace Server.Business.Commons.Response;

public class GetBranchesHasProduct
{
    public int ProductId { get; set; }
    public List<BranchProductDto> Branches { get; set; }
}

public class GetBranchesHasProductResponse<T>
{
    public string Message { get; set; }
    public List<T> Data { get; set; }
}