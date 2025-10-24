using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.Delete;

public record DeleteQuestionCommand(int Id) : IRequest;
