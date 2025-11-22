using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Delete;

public class DeleteTestAssignmentCommandHandler : IRequestHandler<DeleteTestAssignmentCommand>
{
    private readonly ITestAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTestAssignmentCommandHandler(ITestAssignmentRepository assignmentRepository, IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteTestAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken)
                         ?? throw new NotFoundException(nameof(TestAssignment), request.AssignmentId);

        if (assignment.Status != TestAssignmentStatus.Pending)
        {
            throw new Exception("Only 'Pending' assignments can be deleted.");
        }

        // TODO: İlişkili cevapları da silmek gerekebilir (CandidateAnswers)

        _assignmentRepository.Delete(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}