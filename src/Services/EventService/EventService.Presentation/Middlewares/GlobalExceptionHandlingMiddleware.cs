using System.ComponentModel.DataAnnotations;
using EventService.Domain.Exceptions;
using EventService.Presentation.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EventService.Presentation.Middlewares;

public class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await HandleException(httpContext, ex);
        }
    }

    private async Task HandleException(HttpContext httpContext, Exception ex)
    {
        int statusCode = MapStatusCode(ex);

        // Full stack trace only for inner server errors
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                ex,
                "Unhandled exception. Method={Method}, Path={Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        if (httpContext.Response.HasStarted)
        {
            return;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        List<string> details = [];

        if (ex is ModelValidationException validationEx)
        {
            details.AddRange(validationEx.Details);
        }

        var title = ex is DbUpdateException dbEx
            ? dbEx.InnerException switch
            {
                var inner when inner?.Message?.Contains("duplicate key") == true
                    => "Duplicate data detected. Please ensure all unique fields are correct.",
                var inner when inner?.Message?.Contains("foreign key") == true
                    => "The operation references a record that does not exist.",
                var inner when inner?.Message?.Contains("constraint") == true
                    => "The operation violates a database constraint.",
                _ => "A database error occurred. Please try again."
            }
            : ex.Message;

        var error = new ErrorResponseDto
        {
            Title = title,
            Details = details
        };

        await httpContext.Response.WriteAsJsonAsync(error);
    }

    private static int MapStatusCode(Exception ex)
        => ex switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            ForbiddenException => StatusCodes.Status403Forbidden,
            NotFoundException => StatusCodes.Status404NotFound,
            NoAvailableSeatsException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
}
