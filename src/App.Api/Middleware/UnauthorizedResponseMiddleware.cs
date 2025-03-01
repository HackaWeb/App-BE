using App.RestContracts.Shared;
using System.Net;
using System.Text.Json;

namespace App.Api.Middleware;

public class UnauthorizedResponseMiddleware
{
    private readonly RequestDelegate _next;

    public UnauthorizedResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
        {
            context.Response.ContentType = "application/json";
            var response = new ResultResponse()
            {
                ErrorMessage = "You are not authorized.",
                IsSuccess = false,
                StatusCode = 401,
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
