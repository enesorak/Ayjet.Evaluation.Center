namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetList;

public class TestAssignmentDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public string CandidateFullName { get; set; } = string.Empty;
    public string TestTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}