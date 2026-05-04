using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;

namespace NPS.Api.Api.Middleware;

public class ExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
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
            _logger.LogError(ex, "Excepción no manejada en el pipeline HTTP; respuesta JSON emitida al cliente.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            ValidationException validationEx => Application.Common.Models.BaseResponseDto.Failure(
                "Error de validación",
                (int)HttpStatusCode.BadRequest,
                validationEx.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
            ),
            UnauthorizedAccessException => Application.Common.Models.BaseResponseDto.Failure(
                "No autorizado o cuenta bloqueada",
                (int)HttpStatusCode.Unauthorized
            ),
            _ => Application.Common.Models.BaseResponseDto.Failure(
                "Error interno. El detalle quedó registrado en los logs del servidor.",
                (int)HttpStatusCode.InternalServerError
            )
        };

        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonSerializerOptions));
    }
}
