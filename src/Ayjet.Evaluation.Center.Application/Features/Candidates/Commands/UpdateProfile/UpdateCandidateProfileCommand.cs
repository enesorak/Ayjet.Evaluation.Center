using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.UpdateProfile;

public record UpdateCandidateProfileCommand(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string? InitialCode,
    string? FleetCode,
    string? PhoneNumber,
  //  int CandidateType, // <-- EKLENDİ
  //  int Department,    // <-- EKLENDİ
    Gender? Gender,
    DateOnly? BirthDate,
    string? MaritalStatus,
    string? Profession,
    string? EducationLevel
) : IRequest;