using Ayjet.Evaluation.Center.Domain.Enums;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.DTOs;

public class CandidateProfileDto
{
    public Gender? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Profession { get; set; }
    public string? EducationLevel { get; set; }
}