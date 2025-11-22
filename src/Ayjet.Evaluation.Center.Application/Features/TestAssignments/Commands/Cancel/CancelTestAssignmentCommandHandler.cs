using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Cancel;

public class CancelTestAssignmentCommandHandler : IRequestHandler<CancelTestAssignmentCommand>
{
    private readonly ITestAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelTestAssignmentCommandHandler(ITestAssignmentRepository assignmentRepository, IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelTestAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken)
                         ?? throw new NotFoundException(nameof(TestAssignment), request.AssignmentId);

        if (assignment.Status == TestAssignmentStatus.Completed)
        {
            throw new Exception("Cannot cancel a completed assignment.");
        }

        // Durumu Expired olarak ayarla
        assignment.Status = TestAssignmentStatus.Expired;
        assignment.UpdatedAt = DateTime.UtcNow;

        _assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}