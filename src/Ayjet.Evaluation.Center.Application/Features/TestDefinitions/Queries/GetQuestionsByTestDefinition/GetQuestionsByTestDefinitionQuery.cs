using Ayjet.Evaluation.Center.Application.Common.Models;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Queries.GetQuestionsByTestDefinition;

public record GetQuestionsByTestDefinitionQuery(
    string TestDefinitionId,
    string? SearchTerm,
    PaginationParams PageParams // <-- Eklendi
) : IRequest<PagedList<QuestionDto>>;
 