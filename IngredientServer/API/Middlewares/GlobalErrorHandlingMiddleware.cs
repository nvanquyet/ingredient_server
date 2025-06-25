using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IngredientServer.API.Middlewares;
using System.Net;
using System.Text.Json;


public class GlobalErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;

    public GlobalErrorHandlingMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse();

        switch (exception)
        {
            case UnauthorizedAccessException:
                response.Message = "Access denied";
                response.Details = exception.Message;
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                break;
            
            case KeyNotFoundException:
                response.Message = "Resource not found";
                response.Details = exception.Message;
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;
            
            case ArgumentNullException:
            case ArgumentException:
                response.Message = "Invalid request";
                response.Details = exception.Message;
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            
            case InvalidOperationException:
                response.Message = "Operation failed";
                response.Details = exception.Message;
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            
            case HttpRequestException:
                response.Message = "External service unavailable";
                response.Details = exception.Message;
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                break;
            
            default:
                response.Message = "An error occurred while processing the request";
                response.Details = "Internal server error";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        response.StatusCode = context.Response.StatusCode;
        response.Timestamp = DateTime.UtcNow;
        response.Path = context.Request.Path;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Path { get; set; } = string.Empty;
}