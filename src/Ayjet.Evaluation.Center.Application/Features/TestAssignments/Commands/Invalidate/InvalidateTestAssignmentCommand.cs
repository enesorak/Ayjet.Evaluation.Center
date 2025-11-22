using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Invalidate;

public record InvalidateTestAssignmentCommand(
    string AssignmentId,
    string Reason
) : IRequest;