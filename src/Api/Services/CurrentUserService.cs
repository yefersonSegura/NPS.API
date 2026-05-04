using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NPS.Api.Application.Common.Interfaces;

namespace NPS.Api.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return null;

            // JwtBearer puede mapear `sub` y `nameidentifier` al mismo tipo; el primero a veces es el nombre
            // de usuario ("voter01"). Se toma el primer valor que sea entero > 0.
            foreach (var c in user.FindAll(ClaimTypes.NameIdentifier))
            {
                if (TryUserId(c.Value, out var id)) return id;
            }

            // Si `sub` no se fusionó con NameIdentifier (p. ej. MapInboundClaims = false)
            foreach (var c in user.FindAll(JwtRegisteredClaimNames.Sub))
            {
                if (TryUserId(c.Value, out var id)) return id;
            }

            return null;
        }
    }

    private static bool TryUserId(string? value, out int id)
    {
        id = 0;
        return !string.IsNullOrWhiteSpace(value)
            && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out id)
            && id > 0;
    }
}
