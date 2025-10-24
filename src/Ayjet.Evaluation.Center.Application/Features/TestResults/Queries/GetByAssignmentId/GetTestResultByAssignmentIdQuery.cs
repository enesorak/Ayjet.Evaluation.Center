using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetByAssignmentId;

public record GetTestResultByAssignmentIdQuery(string AssignmentId) : IRequest<TestResultDto>;
