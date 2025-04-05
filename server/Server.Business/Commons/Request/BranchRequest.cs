namespace Server.Business.Commons.Request;

public class CreateBranchRequest
{
    public string BranchName { get; set; }

    public string BranchAddress { get; set; }

    public string BranchPhone { get; set; }

    public string? LongAddress { get; set; }

    public string? LatAddress { get; set; }

    public string? Status { get; set; }
    
    public int ManagerId { get; set; }
    
    public int CompanyId { get; set; }
    
    public int? District { get; set; }
    
    public int? WardCode { get; set; }
}

public class UpdateBranchRequest : CreateBranchRequest
{
    public int BranchId { get; set; }
}