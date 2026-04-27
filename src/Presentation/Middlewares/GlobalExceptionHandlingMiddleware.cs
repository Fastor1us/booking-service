using System.ComponentModel.DataAnnotations;
using BookingApi.Domain.Exceptions;
using BookingApi.Presentation.Dtos;

namespace BookingApi.Presentation.Middlewares;

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
        catch (Exception ex)
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

        var error = new ErrorResponseDto
        {
            Title = ex.Message,
            Details = details
        };

        await httpContext.Response.WriteAsJsonAsync(error);
    }

    private static int MapStatusCode(Exception ex)
       => ex switch
       {
           ValidationException => StatusCodes.Status400BadRequest,
           NotFoundException => StatusCodes.Status404NotFound,
           _ => StatusCodes.Status500InternalServerError
       };
}
