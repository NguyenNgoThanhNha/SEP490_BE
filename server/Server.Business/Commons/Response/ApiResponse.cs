namespace Server.Business.Commons.Response;

public class ApiResponse
{
    public string? message { get; set; }
    public object? data { get; set; }

    public static ApiResponse Succeed(object? result, string? message = null)
    {
        return new ApiResponse { data = result, message = message };
    }

    public static ApiResponse Error(string? message = null)
    {
        return new ApiResponse { message = message };
    }
}