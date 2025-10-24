using Ayjet.Evaluation.Center.Application.Common.Models;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Queries.GetList;

// Bu Query işlendiğinde geriye List<TestDefinitionDto> dönecek.
public record GetTestDefinitionListQuery(
    PaginationParams PageParams
) : IRequest<PagedList<TestDefinitionDto>>;