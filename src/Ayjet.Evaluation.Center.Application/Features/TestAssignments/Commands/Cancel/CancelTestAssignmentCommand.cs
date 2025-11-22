using MediatR;
namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Cancel;
public record CancelTestAssignmentCommand(string AssignmentId) : IRequest;