using System.Diagnostics;

namespace EmployeeManagement.Api.Middleware;

/// <summary>
/// Day 7 Deliverable: Custom middleware that safely logs incoming request and outgoing response metadata,
/// measures pipeline execution duration, and attaches an X-Response-Time-Ms header.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestMethod = context.Request.Method;
        var requestPath = context.Request.Path;
        var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;

        _logger.LogInformation("--> Incoming Request: [{Method}] {Path}{Query}", requestMethod, requestPath, queryString);

        // Capture response headers before streaming starts
        context.Response.OnStarting(() =>
        {
            stopwatch.Stop();
            context.Response.Headers.Append("X-Response-Time-Ms", stopwatch.ElapsedMilliseconds.ToString());
            return Task.CompletedTask;
        });

        try
        {
            await _next(context);
        }
        finally
        {
            if (stopwatch.IsRunning)
                stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            _logger.LogInformation("<-- Completed Request: [{Method}] {Path} returned HTTP {StatusCode} in {ElapsedMs}ms",
                requestMethod, requestPath, statusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
