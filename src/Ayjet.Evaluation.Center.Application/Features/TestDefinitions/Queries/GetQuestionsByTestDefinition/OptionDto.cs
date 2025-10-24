namespace Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Queries.GetQuestionsByTestDefinition;

public class OptionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}