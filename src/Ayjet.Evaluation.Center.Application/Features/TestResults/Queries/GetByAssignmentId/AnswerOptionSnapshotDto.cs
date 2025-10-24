namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetByAssignmentId;

public class AnswerOptionSnapshotDto
{
    public int Id { get; set; }
    public string? Text { get; set; }
    public bool IsCorrect { get; set; }
}