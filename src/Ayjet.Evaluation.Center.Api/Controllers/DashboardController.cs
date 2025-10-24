using Ayjet.Evaluation.Center.Application.Features.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayjet.Evaluation.Center.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly ISender _mediator;
    public DashboardController(ISender mediator) => _mediator = mediator;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        return Ok(await _mediator.Send(new GetDashboardStatsQuery()));
    }
}