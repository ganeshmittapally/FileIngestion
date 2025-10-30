using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FileIngestion.Api.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _apiKeyHeader = "X-API-KEY";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var config = context.RequestServices.GetService(typeof(Microsoft.Extensions.Configuration.IConfiguration)) as Microsoft.Extensions.Configuration.IConfiguration;
        var expected = config?["ApiKey"];

        // If no key configured, skip validation (development)
        if (string.IsNullOrEmpty(expected))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(_apiKeyHeader, out var provided) || string.IsNullOrEmpty(provided))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing API Key");
            return;
        }

        if (!string.Equals(provided, expected, System.StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await _next(context);
    }
}
