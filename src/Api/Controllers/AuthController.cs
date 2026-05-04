using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPS.Api.Application.Services.Auth;
using NPS.Api.Application.Common.Models;

namespace NPS.Api.Api.Controllers;

[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<ResponseDto<LoginResponseDto>>> Login(LoginCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ResponseDto<LoginResponseDto>>> Refresh(RefreshTokenCommand command)
    {
        return await Mediator.Send(command);
    }
}
