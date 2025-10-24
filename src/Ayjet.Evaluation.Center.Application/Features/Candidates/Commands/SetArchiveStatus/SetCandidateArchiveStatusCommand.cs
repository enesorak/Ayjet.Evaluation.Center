using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.SetArchiveStatus;

public record SetCandidateArchiveStatusCommand(string CandidateId, bool IsArchived) : IRequest;