using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetTestProgress;

public record GetTestProgressQuery(string AssignmentId) : IRequest<TestProgressDto>;
