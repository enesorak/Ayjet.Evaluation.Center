using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ayjet.Evaluation.Center.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Bu controller'a sadece Admin erişebilir
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesController(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleManager.Roles.Select(r => new { r.Id, r.Name }).ToListAsync();
        return Ok(roles);
    }
}