namespace Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Queries.GetListByTestDefinition;

public class QuestionAnswerOptionDto
{
    public int Id { get; set; }
    public string Text { get; set; }
    public bool IsCorrect { get; set; }
}