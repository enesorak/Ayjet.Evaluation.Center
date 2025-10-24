using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Finish;

public record FinishTestCommand(string AssignmentId) : IRequest;
