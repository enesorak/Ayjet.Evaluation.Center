namespace Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.Update;

public record UpdateAnswerOptionDto
{
    public int? Id { get; set; } // Mevcut seçenekler için ID, yeniler için null
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}