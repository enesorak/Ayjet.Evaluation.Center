using Ayjet.Evaluation.Center.Application.Features.Candidates.DTOs;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetByAssignmentId;

public record GetCandidateByAssignmentIdQuery(string AssignmentId) : IRequest<CandidateDto>;