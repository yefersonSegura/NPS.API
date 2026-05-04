using NPS.Api.Application.Common.Auth;
using NPS.Api.Domain.Entities;

namespace NPS.Api.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Succeeded, string? Token, string? RefreshToken, LoginFailureKind FailureKind)> LoginAsync(
        string username,
        string password);
    Task<(bool Succeeded, string? Token, string? RefreshToken)> RefreshTokenAsync(string token, string refreshToken);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
