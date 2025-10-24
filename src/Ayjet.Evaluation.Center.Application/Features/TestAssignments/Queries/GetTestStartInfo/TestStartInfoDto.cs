namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetTestStartInfo;

public record TestStartInfoDto(
    string CandidateFullName,
    string TestTitle,
    string? TestDescription,
    int? QuestionCount,
    int? TimeLimitInMinutes,
    bool CanStart,
    string StatusMessage,
DateTime? StartedAt 
);