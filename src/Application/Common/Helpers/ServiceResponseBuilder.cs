using NPS.Api.Application.Common.Models;

namespace NPS.Api.Application.Common.Helpers;

public static class ServiceResponseBuilder
{
    public static void ApplyUnexpectedError<T>(ResponseDto<T> response, Exception ex)
    {
        response.Succeeded = false;
        response.StatusCode = 500;
        response.Message = "Ocurrió un error inesperado en la capa de aplicación.";
        
        // Aquí podrías loguear la excepción real o añadirla a la lista de errores
        // según el entorno (Desarrollo vs Producción)
        response.Errors = new List<string> { ex.Message };
    }

    public static void ApplyUnexpectedError(BaseResponseDto response, Exception ex)
    {
        response.Succeeded = false;
        response.StatusCode = 500;
        response.Message = "Ocurrió un error inesperado en la capa de aplicación.";
        response.Errors = new List<string> { ex.Message };
    }
}
