namespace NPS.Api.Application.Common.Auth;

/** Detalle cuando el login no puede emitir JWT (solo para diferenciar mensajes de error). */
public enum LoginFailureKind
{
    None = 0,
    InvalidCredentials = 1,
    AccountLocked = 2,
}
