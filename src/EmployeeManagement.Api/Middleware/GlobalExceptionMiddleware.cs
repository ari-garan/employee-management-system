using System.Net;
using System.Text.Json;
using EmployeeManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Middleware;

/// <summary>
/// Centralized Exception Handling Middleware to transform unhandled or domain-specific exceptions
/// into standardized RFC 7807 ProblemDetails responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing [{Method}] {Path}: {Message}",
                context.Request.Method, context.Request.Path, ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, detail) = exception switch
        {
            EntityNotFoundException enf => (HttpStatusCode.NotFound, "Resource Not Found", enf.Message),
            ValidationException ve => (HttpStatusCode.BadRequest, "Validation Error", ve.Message),
            InsufficientLeaveBalanceException ilb => (HttpStatusCode.BadRequest, "Insufficient Leave Balance", ilb.Message),
            BudgetExceededException be => (HttpStatusCode.BadRequest, "Budget Exceeded", be.Message),
            ArgumentException ae => (HttpStatusCode.BadRequest, "Invalid Argument", ae.Message),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error",
                _env.IsDevelopment() ? exception.ToString() : "An unexpected server error occurred. Please contact support.")
        };

        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        var responseJson = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        return context.Response.WriteAsync(responseJson);
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
