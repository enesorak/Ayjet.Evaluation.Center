using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.PsychometricQuestions.Queries.GetById;

public record GetPsychometricQuestionDetailQuery(int Id) : IRequest<PsychometricQuestionDetailDto>;
