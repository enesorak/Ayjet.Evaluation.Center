using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.MarkPsychologistInterview;

public class MarkPsychologistInterviewCommandHandler : IRequestHandler<MarkPsychologistInterviewCommand>
{
    private readonly ITestAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkPsychologistInterviewCommandHandler(
        ITestAssignmentRepository assignmentRepository, 
        IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkPsychologistInterviewCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken)
                         ?? throw new NotFoundException(nameof(TestAssignment), request.AssignmentId);

        assignment.IsPsychologistInterviewCompleted = true;
        assignment.PsychologistInterviewDate = request.InterviewDate;
        assignment.PsychologistNotes = request.Notes;
        assignment.UpdatedAt = DateTime.UtcNow;

        _assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}