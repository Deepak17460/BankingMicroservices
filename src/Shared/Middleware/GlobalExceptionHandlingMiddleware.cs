using System.Net;
using BankingMicroservices.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BankingMicroservices.Shared.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            CustomerNotFoundException c => (HttpStatusCode.NotFound, "Customer Not Found", c.Message),
            AccountNotFoundException a => (HttpStatusCode.NotFound, "Account Not Found", a.Message),
            InsufficientBalanceException i => (HttpStatusCode.BadRequest, "Insufficient Balance", i.Message),
            ServiceNotFoundException s => (HttpStatusCode.ServiceUnavailable, "Service Unavailable", s.Message),
            ArgumentException a => (HttpStatusCode.BadRequest, "Bad Request", a.Message),
            InvalidOperationException i => (HttpStatusCode.BadRequest, "Invalid Operation", i.Message),
            KeyNotFoundException k => (HttpStatusCode.NotFound, "Not Found", k.Message),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}

public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
}
