namespace Ayjet.Evaluation.Center.Application.Features.Accounts.DTOs;

public record UserProfileDto(
    string Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? ProfilePictureUrl,
    IList<string> Roles
);