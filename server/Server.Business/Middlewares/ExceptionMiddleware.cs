using Microsoft.AspNetCore.Http;
using Server.Business.Commons;
using Server.Business.Exceptions;
using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Middlewares
{
public class ExceptionMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IMapper _mapper;
    private readonly UnitOfWorks _unitOfWorks;

    private readonly IDictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;

    public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger, IMapper mapper, UnitOfWorks unitOfWorks)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _unitOfWorks = unitOfWorks ?? throw new ArgumentNullException(nameof(unitOfWorks));

        _exceptionHandlers = new Dictionary<Type, Func<HttpContext, Exception, Task>>
        {
            { typeof(NotFoundException), HandleNotFoundExceptionAsync },
            { typeof(BadRequestException), HandleBadRequestExceptionAsync },
            { typeof(UnAuthorizedException), HandleUnAuthorizedExceptionAsync },
            { typeof(RequestValidationException), HandleRequestValidationExceptionAsync },
        };
    }

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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        if (_exceptionHandlers.TryGetValue(ex.GetType(), out var handler))
        {
            await handler(context, ex);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteExceptionMessageAsync(context, ex);
        }
    }

    private async Task HandleNotFoundExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await WriteExceptionMessageAsync(context, ex);
    }

    private async Task HandleBadRequestExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await WriteExceptionMessageAsync(context, ex);
    }

    private async Task HandleUnAuthorizedExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await WriteExceptionMessageAsync(context, ex);
    }

    private async Task HandleRequestValidationExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        if (ex is RequestValidationException validationException)
        {
            var result = ApiResult<Dictionary<string, string[]>>.Fail(validationException) with
            {
                Result = validationException.ProblemDetails.Errors,
            };
            await context.Response.Body.WriteAsync(SerializeToUtf8BytesWeb(result));
        }
    }

    private async Task WriteExceptionMessageAsync(HttpContext context, Exception ex)
    {
        var logger = new LoggerModel
        {
            Message = ex?.Message ?? "Unknown error",
            Exception = ex?.StackTrace,
            Status = "Error"
        };

        var loggerEntity = _mapper.Map<Logger>(logger);
        await _unitOfWorks.LoggerRepository.AddAsync(loggerEntity);
        await _unitOfWorks.LoggerRepository.Commit();

        await context.Response.Body.WriteAsync(SerializeToUtf8BytesWeb(ApiResult<string>.Fail(ex)));
    }

    private static byte[] SerializeToUtf8BytesWeb<T>(T value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}

}
