using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.Create;

public record CreateCandidateCommand(
    string FirstName,
    string LastName,
    string Email,
    int Department,
    int CandidateType,
    int? Gender,
    string? InitialCode,       // <-- string? olmalı
    string? FleetCode,         // <-- string? olmalı
    DateOnly? BirthDate,       // <-- Eklendi
    string? MaritalStatus,
    string? Profession,
    string? EducationLevel
) : IRequest<string>;