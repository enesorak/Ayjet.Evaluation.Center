namespace Ayjet.Evaluation.Center.Application.Features.Accounts.DTOs;

public class RegisterDto {
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new(); // veya new List<string>();

}