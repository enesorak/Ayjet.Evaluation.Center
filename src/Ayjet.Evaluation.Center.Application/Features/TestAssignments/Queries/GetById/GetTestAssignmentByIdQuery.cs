using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetById;

public record GetTestAssignmentByIdQuery(string Id) : IRequest<TestAssignmentDetailDto>;
