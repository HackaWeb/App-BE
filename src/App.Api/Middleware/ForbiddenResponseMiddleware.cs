using App.RestContracts.Shared;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace App.Api.Middleware;

public class ForbiddenResponseMiddleware
{
    private readonly RequestDelegate _next;

    public ForbiddenResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
        {
            context.Response.ContentType = "application/json";
            var response = new ResultResponse()
            {
                ErrorMessage = "You do not have access to this facility.", StatusCode = 403, IsSuccess = false,
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
