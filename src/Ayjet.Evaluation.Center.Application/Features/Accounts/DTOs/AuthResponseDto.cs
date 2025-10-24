namespace Ayjet.Evaluation.Center.Application.Features.Accounts.DTOs;

public class AuthResponseDto {
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}