using Microsoft.AspNetCore.Diagnostics;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Middlewares;

/// <summary>
/// Global exception handler mapping domain/validation exceptions to standardized HTTP status codes and JSON responses.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, error) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred for {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.Headers.Append("X-Error-Type", exception.GetType().Name);

        var message = statusCode == StatusCodes.Status500InternalServerError
            ? "Đã có lỗi xảy ra ở máy chủ. Vui lòng thử lại sau."
            : exception.Message;

        await httpContext.Response.WriteAsJsonAsync(new
        {
            statusCode,
            error,
            message
        }, cancellationToken);

        return true;
    }
}
