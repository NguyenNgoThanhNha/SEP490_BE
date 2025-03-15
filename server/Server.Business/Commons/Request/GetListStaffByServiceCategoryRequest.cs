namespace Server.Business.Commons.Request;

public class GetListStaffByServiceCategoryRequest
{
    public int BranchId { get; set; }
    
    public int[] ServiceCategoryIds { get; set; }
}