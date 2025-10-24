using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.Delete;

public record DeleteCandidateCommand(string Id) : IRequest;