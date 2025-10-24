using AAyjet.Evaluation.Center.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ayjet.Evaluation.Center.Api.Controllers;

[ApiController]
[Route("api/psychometric-scales")]
[Authorize(Roles = "Admin")]
public class PsychometricScalesController : ControllerBase
{
    private readonly ApplicationDbContext _context; // Basit bir okuma olduğu için doğrudan context
    public PsychometricScalesController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetScales()
    {
        var scales = await _context.PsychometricScales
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync();
        return Ok(scales);
    }
}