using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.Delete;

public class DeleteCandidateCommandHandler(
    ICandidateRepository candidateRepo,
    ITestAssignmentRepository assignmentRepo,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCandidateCommand>
{
    public async Task Handle(DeleteCandidateCommand request, CancellationToken cancellationToken)
    {
        var candidate = await candidateRepo.GetByIdAsync(request.Id, cancellationToken)
                        ?? throw new NotFoundException(nameof(Candidate), request.Id);

        // 1. Adayın aktif bir testi var mı diye kontrol et
        var hasActiveAssignments = await assignmentRepo.AnyAsync(a => a.CandidateId == request.Id 
                                                                       && (a.Status == TestAssignmentStatus.InProgress || a.Status == TestAssignmentStatus.Pending), 
            cancellationToken);

        // 2. Duruma göre sil veya arşivle
        if (hasActiveAssignments)
        {
            // Aktif testi varsa, SADECE ARŞİVLE
            candidate.IsArchived = true;
            candidateRepo.Update(candidate);
        }
        else
        {
            // Aktif testi yoksa, KALICI OLARAK SİL
            candidateRepo.Delete(candidate);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}