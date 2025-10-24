using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetById;

public record GetCandidateByIdQuery(string Id) : IRequest<CandidateDto>;