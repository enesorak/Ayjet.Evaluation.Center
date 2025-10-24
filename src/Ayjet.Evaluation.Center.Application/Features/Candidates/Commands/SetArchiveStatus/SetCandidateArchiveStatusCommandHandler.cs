using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.SetArchiveStatus;

public class SetCandidateArchiveStatusCommandHandler : IRequestHandler<SetCandidateArchiveStatusCommand>
{
    private readonly ICandidateRepository _candidateRepo;
    private readonly IUnitOfWork _unitOfWork;
    public SetCandidateArchiveStatusCommandHandler(ICandidateRepository repo, IUnitOfWork uow) 
        => (_candidateRepo, _unitOfWork) = (repo, uow);

    public async Task Handle(SetCandidateArchiveStatusCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _candidateRepo.GetByIdAsync(request.CandidateId, cancellationToken)
                        ?? throw new NotFoundException(nameof(Candidate), request.CandidateId);

        candidate.IsArchived = request.IsArchived;
        candidate.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}