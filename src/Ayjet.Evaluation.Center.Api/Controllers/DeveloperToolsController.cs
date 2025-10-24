using Ayjet.Evaluation.Center.Application.Features.DeveloperTools;
 using Ayjet.Evaluation.Center.Application.Features.DeveloperTools.SeedCandidates; // <-- Yeni using
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ayjet.Evaluation.Center.Api.Controllers;

[ApiController]
[Route("api/dev-tools")]
public class DeveloperToolsController : ControllerBase
{
    private readonly ISender _mediator;
    public DeveloperToolsController(ISender mediator) => _mediator = mediator;

    [HttpPost("run-mmpi-simulation")]
    public async Task<IActionResult> RunMmpiSimulation()
    {
        var result = await _mediator.Send(new RunMMPISimulationCommand());
        return Ok(result);
    }

    // --- YENİ ENDPOINT ---
    [HttpPost("seed-candidates/{count:int}")]
    public async Task<IActionResult> SeedCandidates(int count)
    {
        if (count <= 0 || count > 100)
        {
            return BadRequest("Lütfen 1 ile 100 arasında bir sayı girin.");
        }
        var createdCount = await _mediator.Send(new SeedCandidatesCommand(count));
        return Ok($"{createdCount} adet rastgele aday başarıyla oluşturuldu.");
    }
}