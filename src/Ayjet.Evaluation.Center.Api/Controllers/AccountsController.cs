using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ayjet.Evaluation.Center.Application.Features.Accounts.Commands.UpdateProfilePicture;
using Ayjet.Evaluation.Center.Application.Features.Accounts.Commands.UpdateRoles;
using Ayjet.Evaluation.Center.Application.Features.Accounts.DTOs;
using Ayjet.Evaluation.Center.Application.Features.Accounts.Queries.GetList;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Ayjet.Evaluation.Center.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ISender _mediator;

    public AccountsController(UserManager<ApplicationUser> userManager, IConfiguration configuration, ISender mediator)
    {
        _userManager = userManager;
        _configuration = configuration;
        _mediator = mediator;
    }
    
    
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var dto = new UserProfileDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Email!,
            user.ProfilePictureUrl,
            roles
        );
        return Ok(dto);
    }

    [HttpPost("login")]
    [AllowAnonymous] // Bu endpoint'e herkes erişebilir
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            var token = GenerateJwtToken(user, await _userManager.GetRolesAsync(user));
            return Ok(token);
        }
        return Unauthorized();
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")] // Sadece Admin yeni kullanıcı kaydedebilir
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        if (model.Roles != null && model.Roles.Any())
            await _userManager.AddToRolesAsync(user, model.Roles);

        return Ok(new { Message = "User registered successfully!" });
    }

    // Bu metodu controller'ın en altına özel bir metot olarak ekleyin
    private AuthResponseDto GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(7); // Token 7 gün geçerli

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new AuthResponseDto {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires
        };
    }
    
    [HttpPost("profile-picture")]
    [Authorize] // Sadece giriş yapmış kullanıcılar kendi resmini yükleyebilir
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        await using var stream = file.OpenReadStream();
        
        var command = new UpdateProfilePictureCommand(userId, stream, file.FileName);
        var fileUrl = await _mediator.Send(command);

        return Ok(new { url = fileUrl });
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserList()
    {
        return Ok(await _mediator.Send(new GetUserListQuery()));
    }
    
    
    [HttpPost("{userId}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUserRoles(string userId, [FromBody] List<string> roles)
    {
        await _mediator.Send(new UpdateUserRolesCommand(userId, roles));
        return NoContent();
    }
}