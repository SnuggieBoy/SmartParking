using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Models;
using SmartParking.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace SmartParking.API.Middlewares;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var isDevelopment = _environment.IsDevelopment();

        // SECURITY: Known business exceptions - safe to expose
        var (statusCode, message, errors) = exception switch
        {
            BusinessException businessEx => (
                businessEx.StatusCode,
                businessEx.Message,
                businessEx.Errors?.ToArray() ?? new[] { businessEx.Message }
            ),
            BadRequestException badRequestEx => (
                HttpStatusCode.BadRequest,
                badRequestEx.Message,
                new[] { badRequestEx.Message }
            ),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                notFoundEx.Message,
                new[] { notFoundEx.Message }
            ),
            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                unauthorizedEx.Message,
                new[] { unauthorizedEx.Message }
            ),
            UnauthorizedAccessException uaEx => (
                HttpStatusCode.Unauthorized,
                uaEx.Message,
                new[] { uaEx.Message }
            ),
            DbUpdateException dbEx => (
                HttpStatusCode.InternalServerError,
                "Database update failed. Please ensure the latest migrations have been applied to the database.",
                isDevelopment
                    ? new[] {
                        dbEx.Message,
                        dbEx.InnerException != null ? $"Inner Exception: {dbEx.InnerException.Message}" : ""
                    }
                    : Array.Empty<string>()
            ),
            // SECURITY: Unknown exceptions - sanitize in production
            _ => isDevelopment
                ? (
                    HttpStatusCode.InternalServerError,
                    exception.Message,
                    new[] { 
                        exception.StackTrace ?? exception.Message,
                        exception.InnerException != null 
                            ? $"Inner Exception: {exception.InnerException.Message}\n{exception.InnerException.StackTrace}" 
                            : ""
                    }
                )
                : (
                    HttpStatusCode.InternalServerError,
                    Messages.Common.InternalServerError,  // Generic message only
                    Array.Empty<string>()  // No error details
                )
        };

        context.Response.StatusCode = (int)statusCode;

        // ALWAYS log full exception server-side (even in production)
        var innerException = exception.InnerException != null 
            ? $" Inner: {exception.InnerException.Message}" 
            : "";
        _logger.LogError(exception,
            "Unhandled exception: {Message}{InnerException}. Path: {Path}. User: {User}",
            exception.Message,
            innerException,
            context.Request.Path,
            context.User.Identity?.Name ?? "Anonymous");

        var response = ApiResponse<object>.FailureResponse(message, errors);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = isDevelopment  // Pretty-print only in development
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
