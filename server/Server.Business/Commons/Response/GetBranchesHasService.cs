using Server.Business.Dtos;

namespace Server.Business.Commons.Response;

public class GetBranchesHasService
{
    public int ServiceId { get; set; }
    public List<BranchServiceDto> Branches { get; set; }
}

public class GetBranchesHasServiceResponse<T>
{
    public string Message { get; set; }
    public List<T> Data { get; set; }
}