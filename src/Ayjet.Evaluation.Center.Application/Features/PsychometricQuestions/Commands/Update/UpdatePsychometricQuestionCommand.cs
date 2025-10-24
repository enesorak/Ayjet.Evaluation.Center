using Ayjet.Evaluation.Center.Application.Features.PsychometricQuestions.Queries.GetById;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.PsychometricQuestions.Commands.Update;

public record UpdatePsychometricQuestionCommand(
    int Id,
    int DisplayOrder,
  
    bool IsActive,
    List<TranslationDto> Translations,
    List<ScaleMappingDto> ScaleMappings
) : IRequest;