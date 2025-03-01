using App.Domain.Exceptions;
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
            await HandleGenericExceptionAsync(context);
        }
    }

    private static Task HandleDomainExceptionAsync(HttpContext context, DomainException exception)
    {
        var statusCode = exception.InternalStatusCode ?? (int)HttpStatusCode.BadRequest;

        var response = new
        {
            statusCode,
            message = exception.Message,
            additionalInfo = exception.AdditionalInfo.Any() ? exception.AdditionalInfo : null
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(jsonResponse);
    }

    private static Task HandleGenericExceptionAsync(HttpContext context)
    {
        var response = new
        {
            statusCode = (int)HttpStatusCode.InternalServerError,
            message = "An unexpected error occurred."
        };

        var jsonResponse = JsonSerializer.Serialize(response, JsonOptions);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsync(jsonResponse);
    }
}
