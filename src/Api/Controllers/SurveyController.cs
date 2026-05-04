using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPS.Api.Application.Services.Survey;
using NPS.Api.Application.Common.Models;

namespace NPS.Api.Api.Controllers;

[Authorize(Roles = "2")] // Voter
public class SurveyController : ApiControllerBase
{
    [HttpPost("vote")]
    public async Task<ActionResult<BaseResponseDto>> Vote(CreateSurveyResponseCommand command)
    {
        return await Mediator.Send(command);
    }
}
