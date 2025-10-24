using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetList;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetListByCandidate;

public record GetAssignmentsByCandidateQuery(string CandidateId) : IRequest<List<TestAssignmentDto>>;