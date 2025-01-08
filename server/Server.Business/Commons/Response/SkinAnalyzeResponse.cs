using Server.Business.Models;

namespace Server.Business.Commons.Response;

public class SkinAnalyzeResponse
{
    public string message { get; set; }
    
    public ApiSkinAnalyzeResponse data { get; set; }
}

public class ApiSkinAnalyzeResponse
{
    public object skinhealth { get; set; }
    public List<SkincareRoutineModel> routines { get; set; }
}