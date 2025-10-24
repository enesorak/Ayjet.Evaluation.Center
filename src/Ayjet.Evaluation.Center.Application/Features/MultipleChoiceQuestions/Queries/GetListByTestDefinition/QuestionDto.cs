namespace Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Queries.GetListByTestDefinition;

public class QuestionDto
{
    public int Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public string? QuestionCode { get; init; } // <-- YENİ ALAN
    public int DisplayOrder { get; init; }     // <-- YENİ ALAN
    public List<QuestionAnswerOptionDto> Options { get; init; } = new();
}