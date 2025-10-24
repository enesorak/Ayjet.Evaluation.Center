using Ayjet.Evaluation.Center.Application.Common.Models;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList;

public record GetCandidateListQuery(
    PaginationParams PageParams,
    string? SearchTerm,
    Department? Department,
    bool? IsArchived,
    TestAssignmentStatus? AssignmentStatus // <-- YENİ FİLTRE PARAMETRESİ
) : IRequest<PagedList<CandidateDto>>;