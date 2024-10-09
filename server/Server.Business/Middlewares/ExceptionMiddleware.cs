using Microsoft.AspNetCore.Http;
using Server.Business.Commons;
using Server.Business.Exceptions;
using System.Text.Json;

namespace Server.Business.Middlewares
{
    public class ExceptionMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private readonly IDictionary<Type, Action<HttpContext, Exception>> _exceptionHandlers = new Dictionary<Type, Action<HttpContext, Exception>>
    {
        // Note: Handle every exception you throw here
        
        // a NotFoundException is thrown when the resource requested by the client
        // cannot be found on the resource server
        { typeof(NotFoundException), HandleNotFoundException },
        { typeof(BadRequestException), HandleBadRequestException },
        { typeof(UnAuthorizedException), HandleUnAuthorizedException },
        { typeof(RequestValidationException), HandleRequestValidationException },
    };

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var type = ex.GetType();
            if (_exceptionHandlers.TryGetValue(type, out var handler))
            {
                handler.Invoke(context, ex);
                return Task.CompletedTask;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            Console.WriteLine(ex.ToString());
            return Task.CompletedTask;
        }


        private static async void HandleNotFoundException(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await WriteExceptionMessageAsync(context, ex);
        }

        private static async void HandleBadRequestException(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteExceptionMessageAsync(context, ex);
        }

        private static async void HandleUnAuthorizedException(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await WriteExceptionMessageAsync(context, ex);
        }

        private static async void HandleRequestValidationException(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var exception = ex as RequestValidationException;
            var result = ApiResult<Dictionary<string, string[]>>.Fail(exception!) with
            {
                Result = exception!.ProblemDetails.Errors,
            };
            await context.Response.Body.WriteAsync(SerializeToUtf8BytesWeb(result));
        }


        private static async Task WriteExceptionMessageAsync(HttpContext context, Exception ex)
        {
            await context.Response.Body.WriteAsync(SerializeToUtf8BytesWeb(ApiResult<string>.Fail(ex)));
        }

        private static byte[] SerializeToUtf8BytesWeb<T>(T value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
}
