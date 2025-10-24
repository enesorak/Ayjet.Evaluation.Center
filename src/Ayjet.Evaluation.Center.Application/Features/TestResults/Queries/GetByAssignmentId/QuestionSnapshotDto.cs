namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetByAssignmentId;

public class QuestionSnapshotDto
{
    public string? QuestionText { get; set; }
    public List<AnswerOptionSnapshotDto> Options { get; set; } = new();
    public int? SelectedOptionId { get; set; }
}