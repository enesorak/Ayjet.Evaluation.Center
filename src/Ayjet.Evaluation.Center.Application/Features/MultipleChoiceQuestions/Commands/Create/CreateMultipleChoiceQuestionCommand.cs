using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.Create;

public record CreateMultipleChoiceQuestionCommand(
    string? TestDefinitionId,
    string Text,
    string Language,
    int DifficultyLevel,
    List<AnswerOptionDto> Options,
    string? QuestionCode, // <-- Ekle
    int? DisplayOrder    // <-- Ekle
) : IRequest<int>;