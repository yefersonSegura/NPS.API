using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPS.Api.Application.Services.Survey;
using NPS.Api.Application.Common.Models;

namespace NPS.Api.Api.Controllers;

[Authorize(Roles = "1")] // Admin
public class AdminController : ApiControllerBase
{
    [HttpGet("results")]
    public async Task<ActionResult<ResponseDto<NpsResultsDto>>> GetResults()
    {
        return await Mediator.Send(new GetNpsResultsQuery());
    }
}
