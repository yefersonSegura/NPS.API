using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using NPS.Api.Application.Common.Auth;
using NPS.Api.Application.Common.Interfaces;
using NPS.Api.Application.Common.Models;
using NPS.Api.Application.Common.Helpers;

namespace NPS.Api.Application.Services.Auth;

public record LoginCommand(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password)
    : IRequest<ResponseDto<LoginResponseDto>>;

public record LoginResponseDto(string Token, string RefreshToken);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginHandler : IRequestHandler<LoginCommand, ResponseDto<LoginResponseDto>>
{
    private readonly IIdentityService _identity;

    public LoginHandler(IIdentityService identity) => _identity = identity;

    public async Task<ResponseDto<LoginResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var response = new ResponseDto<LoginResponseDto>();

        try
        {
            var (succeeded, token, refresh, failure) = await _identity.LoginAsync(request.Username, request.Password);

            if (!succeeded || token is null || refresh is null)
            {
                var msg = failure == LoginFailureKind.AccountLocked
                    ? "Cuenta bloqueada por 3 intentos fallidos. Espera 5 minutos o vuelve a ejecutar Scripts/SeedData.sql si necesitas restablecer la cuenta de prueba."
                    : "Usuario o contraseña incorrectos. Contraseñas de prueba están comentadas en Scripts/SeedData.sql.";
                return ResponseDto<LoginResponseDto>.Failure(msg, 401);
            }

            response.Data = new LoginResponseDto(token, refresh);
            response.Succeeded = true;
            response.Message = "Login exitoso";
            response.StatusCode = 200;

            return response;
        }
        catch (Exception ex)
        {
            ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
            response.Data = null;
            return response;
        }
    }
}
