using Ecommerce.Application.Common.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace Ecommerce.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            var logLevel = ex switch
            {
                ValidationException or BadRequestException or NotFoundException => LogLevel.Warning,
                _ => LogLevel.Error
            };

            _logger.Log(logLevel, ex, "Request error: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ValidationException ex => (HttpStatusCode.BadRequest, string.Join("; ", ex.Errors.Select(e => e.ErrorMessage))),
            BadRequestException ex => (HttpStatusCode.BadRequest, ex.Message),
            NotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An internal server error occurred.")
        };

        response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new
        {
            success = false,
            message,
            errors = exception is ValidationException vEx
                ? vEx.Errors.Select(e => e.ErrorMessage).ToList()
                : new List<string> { message }
        });

        await response.WriteAsync(result);
    }
}
