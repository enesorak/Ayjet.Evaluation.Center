using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.MarkPsychologistInterview;

public record MarkPsychologistInterviewCommand(
    string AssignmentId,
    DateTime InterviewDate,
    string? Notes
) : IRequest;