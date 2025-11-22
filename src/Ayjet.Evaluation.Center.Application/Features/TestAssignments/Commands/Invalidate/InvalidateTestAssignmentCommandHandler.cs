using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Invalidate;

public class InvalidateTestAssignmentCommandHandler : IRequestHandler<InvalidateTestAssignmentCommand>
{
    private readonly ITestAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InvalidateTestAssignmentCommandHandler(
        ITestAssignmentRepository assignmentRepository, 
        IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(InvalidateTestAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken)
                         ?? throw new NotFoundException(nameof(TestAssignment), request.AssignmentId);

        // Sadece Completed durumundaki sınavlar geçersiz sayılabilir
        if (assignment.Status != TestAssignmentStatus.Completed)
        {
            throw new InvalidOperationException(
                "Only completed tests can be invalidated."
            );
        }

        // Status'u Invalidated yap ve bilgileri kaydet
        assignment.Status = TestAssignmentStatus.Invalidated;
        assignment.InvalidatedAt = DateTime.UtcNow;
        assignment.InvalidationReason = request.Reason;
        assignment.UpdatedAt = DateTime.UtcNow;

        _assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}