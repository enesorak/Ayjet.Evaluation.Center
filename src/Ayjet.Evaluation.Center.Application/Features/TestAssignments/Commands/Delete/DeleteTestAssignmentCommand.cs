using MediatR;
namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Delete;
public record DeleteTestAssignmentCommand(string AssignmentId) : IRequest;