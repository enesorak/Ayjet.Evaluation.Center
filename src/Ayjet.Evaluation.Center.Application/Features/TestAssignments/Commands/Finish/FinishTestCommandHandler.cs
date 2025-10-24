using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using Hangfire;
using MediatR;

// <-- Ekle

// <-- Ekle
namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Finish;

public class FinishTestCommandHandler : IRequestHandler<FinishTestCommand>
{
    private readonly ITestAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    private readonly IBackgroundJobClient _backgroundJobClient;

    public FinishTestCommandHandler(ITestAssignmentRepository assignmentRepository, IUnitOfWork unitOfWork, IBackgroundJobClient backgroundJobClient)
    {
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
 
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task Handle(FinishTestCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken)
                         ?? throw new NotFoundException(nameof(TestAssignment), request.AssignmentId);

        if (assignment.Status != TestAssignmentStatus.InProgress)
        {
            // Zaten bitmişse bir şey yapma
            return;
        }

        // 1. Atamanın durumunu ve bitiş zamanını hemen güncelle
        assignment.Status = TestAssignmentStatus.Completed;
        assignment.CompletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 2. Puanlama işini bir arka plan görevine devret
        // Not: .Schedule ile 5 dakika sonraya da kurabilirdik, .Enqueue anında başlatır.
        _backgroundJobClient.Enqueue<IScoringService>(service => 
            service.CalculateAndSaveScoreAsync(request.AssignmentId));
    }
}