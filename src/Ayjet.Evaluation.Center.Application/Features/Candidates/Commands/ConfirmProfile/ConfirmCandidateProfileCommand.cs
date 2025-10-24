using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.ConfirmProfile;

public record ConfirmCandidateProfileCommand(string CandidateId) : IRequest;