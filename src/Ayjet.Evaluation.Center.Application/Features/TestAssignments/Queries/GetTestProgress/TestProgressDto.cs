using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList;
using Ayjet.Evaluation.Center.Domain.Enums;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetTestProgress;

public class TestProgressDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public CandidateDto Candidate { get; set; } = null!;
    public string TestTitle { get; set; } = string.Empty;
    public int? TimeLimitInMinutes { get; set; }
    public DateTime? StartedAt { get; set; }
    public TestType TestType { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<QuestionProgressDto> Questions { get; set; } = new();
}