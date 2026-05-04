using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NPS.Api.Application.Common.Interfaces;
using NPS.Api.Application.Common.Auth;
using NPS.Api.Domain.Entities;
using BC = BCrypt.Net.BCrypt;

namespace NPS.Api.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _config;

    public IdentityService(IUserRepository users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    public async Task<(bool Succeeded, string? Token, string? RefreshToken, LoginFailureKind FailureKind)> LoginAsync(
        string user,
        string pass)
    {
        user = user.Trim();
        var account = await _users.GetByUsernameAsync(user);

        // No damos pistas de si el usuario existe o no
        if (account is null) return (false, null, null, LoginFailureKind.InvalidCredentials);

        // LockoutEnd desde SQL suele venir como DateTimeKind.Unspecified; lo tratamos como UTC (lo guardamos con UtcNow).
        var lockoutUtc = ToUtc(account.LockoutEnd);
        if (lockoutUtc.HasValue)
        {
            if (lockoutUtc.Value <= DateTime.UtcNow)
            {
                // Bloqueo vencido: permitir reintentos sin quedar con FailedAttempts "pegados"
                account.FailedAttempts = 0;
                account.LockoutEnd = null;
                await _users.UpdateAsync(account);
            }
            else
            {
                return (false, null, null, LoginFailureKind.AccountLocked);
            }
        }

        if (!VerifyPassword(pass, account.PasswordHash))
        {
            await RegisterFailedAttempt(account);
            return (false, null, null, LoginFailureKind.InvalidCredentials);
        }

        // Llegamos bien: limpiamos intentos y firmamos access + refresh nuevos.
        account.FailedAttempts = 0;
        account.LockoutEnd = null;

        var accessToken = CreateJwtToken(account);
        var refreshToken = CreateRefreshToken();

        account.RefreshToken = refreshToken;
        account.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        
        await _users.UpdateAsync(account);

        return (true, accessToken, refreshToken, LoginFailureKind.None);
    }

    private async Task RegisterFailedAttempt(User account)
    {
        account.FailedAttempts++;
        if (account.FailedAttempts >= 3)
        {
            account.LockoutEnd = DateTime.UtcNow.AddMinutes(5);
        }
        await _users.UpdateAsync(account);
    }

    public async Task<(bool Succeeded, string? Token, string? RefreshToken)> RefreshTokenAsync(string token, string refreshToken)
    {
        var account = await _users.GetByRefreshTokenAsync(refreshToken);

        if (account is null || account.RefreshTokenExpiry < DateTime.UtcNow)
            return (false, null, null);

        var newToken = CreateJwtToken(account);
        var newRefresh = CreateRefreshToken();

        account.RefreshToken = newRefresh;
        account.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        
        await _users.UpdateAsync(account);

        return (true, newToken, newRefresh);
    }

    public string HashPassword(string password) => BC.HashPassword(password);

    public bool VerifyPassword(string password, string hash)
    {
        hash = hash?.Trim() ?? string.Empty;
        if (hash.Length == 0) return false;
        return BC.Verify(password, hash);
    }

    private string CreateJwtToken(User user)
    {
        var settings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings["Secret"]!));
        
        // `sub` se mapea por defecto a NameIdentifier; debe ser estable y parseable como UserId int.
        // El nombre para mostrar/autorización por usuario usa ClaimTypes.Name.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.RoleId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: settings["Issuer"],
            audience: settings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(settings["ExpiryMinutes"]!)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static DateTime? ToUtc(DateTime? value)
    {
        if (!value.HasValue) return null;
        var dt = value.Value;
        return dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
    }

    private string CreateRefreshToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
