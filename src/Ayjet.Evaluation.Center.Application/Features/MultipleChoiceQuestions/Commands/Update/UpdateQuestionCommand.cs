using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.Update;

// UpdateQuestionCommand.cs
public record UpdateQuestionCommand : IRequest
{
    public int Id { get; set; } // Hangi sorunun güncelleneceği
    public string Text { get; set; } = string.Empty;
    public string? QuestionCode { get; set; }
    public int DisplayOrder { get; set; }
    public int DifficultyLevel { get; set; }
    public List<UpdateAnswerOptionDto> Options { get; set; } = new();
}