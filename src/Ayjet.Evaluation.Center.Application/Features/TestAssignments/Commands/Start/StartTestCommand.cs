using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Start;

public record StartTestCommand(string AssignmentId) : IRequest;
