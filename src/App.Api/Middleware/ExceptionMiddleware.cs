using App.Domain.Exceptions;
using App.RestContracts.Shared;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace App.Api.Middleware;

public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException domainException)
        {
            logger.LogError(domainException, "Domain exception occurred: {Message}", domainException.Message);
            await HandleDomainExceptionAsync(context, domainException);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred.");
            await HandleGenericExceptionAsync(context, ex.Message);
        }
    }

    private static Task HandleDomainExceptionAsync(HttpContext context, DomainException exception)
    {
        var statusCode = exception.InternalStatusCode ?? (int)HttpStatusCode.BadRequest;

        var response = new ResultResponse
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = exception.Message,
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(jsonResponse);
    }

    private static Task HandleGenericExceptionAsync(HttpContext context, string details)
    {
        var response = new ResultResponse
        {
            IsSuccess = false,
            StatusCode = (int)HttpStatusCode.InternalServerError,
            ErrorMessage = $"An unexpected error occurred. Details: {details}"
        };

        var jsonResponse = JsonSerializer.Serialize(response, JsonOptions);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsync(jsonResponse);
    }
}
