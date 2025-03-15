using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class GetListStaffByServiceCategoryResponse
{
    public string Message { get; set; }
    public StaffServiceCategoryResponse[] Data { get; set; }
}

public class StaffServiceCategoryResponse
{
    public int ServiceCategoryId { get; set; }
    
    public List<StaffModel>? Staffs { get; set; }
}