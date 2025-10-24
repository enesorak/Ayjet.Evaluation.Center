using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.ConfirmProfile;

public class ConfirmCandidateProfileCommandHandler(IUnitOfWork unitOfWork, ICandidateRepository candidateRepo)
    : IRequestHandler<ConfirmCandidateProfileCommand>
{
 
    public async Task Handle(ConfirmCandidateProfileCommand request, CancellationToken cancellationToken)
    {
        var candidate = await candidateRepo.GetByIdAsync(request.CandidateId, cancellationToken)
                        ?? throw new NotFoundException(nameof(Candidate), request.CandidateId);

        candidate.IsProfileConfirmed = true;
        candidate.ProfileConfirmedAt = DateTime.UtcNow;

        candidateRepo.Update(candidate);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}