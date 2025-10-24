using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetTestStartInfo;

public record GetTestStartInfoQuery(string AssignmentId) : IRequest<TestStartInfoDto>;
