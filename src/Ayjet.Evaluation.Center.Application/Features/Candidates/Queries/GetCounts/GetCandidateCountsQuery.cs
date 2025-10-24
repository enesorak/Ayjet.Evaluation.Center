using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetCounts;

public record GetCandidateCountsQuery() : IRequest<CandidateCountsDto>;
