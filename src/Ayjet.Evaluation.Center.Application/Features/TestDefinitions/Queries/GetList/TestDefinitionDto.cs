namespace Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Queries.GetList;

public record TestDefinitionDto(
    string Id,
    string Title,
    string Description,
    string Type,
    int? DefaultTimeLimitInMinutes,
    int? DefaultQuestionCount,
    int? PassingScore 
);