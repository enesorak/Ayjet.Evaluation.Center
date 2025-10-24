using Ayjet.Evaluation.Center.Application.Common.Models;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetList;

public record GetAssignmentListQuery(
    TestAssignmentStatus? Status, 
    PaginationParams PageParams) : IRequest<PagedList<TestAssignmentDto>>;
