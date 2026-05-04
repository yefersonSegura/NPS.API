using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using NPS.Api.Application.Common.Interfaces;
using NPS.Api.Application.Common.Models;
using NPS.Api.Application.Common.Helpers;

namespace NPS.Api.Application.Services.Auth;

public record RefreshTokenCommand(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("refreshToken")] string RefreshToken)
    : IRequest<ResponseDto<LoginResponseDto>>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, ResponseDto<LoginResponseDto>>
{
    private readonly IIdentityService _identity;

    public RefreshTokenHandler(IIdentityService identity) => _identity = identity;

    public async Task<ResponseDto<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var response = new ResponseDto<LoginResponseDto>();

        try
        {
            var (succeeded, token, refresh) = await _identity.RefreshTokenAsync(request.Token, request.RefreshToken);

            if (!succeeded || token is null || refresh is null)
                return ResponseDto<LoginResponseDto>.Failure("Token de actualización inválido o caducado.", 401);

            response.Data = new LoginResponseDto(token, refresh);
            response.Succeeded = true;
            response.Message = "Tokens renovados";
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
