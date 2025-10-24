using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Commands.Delete;

public class DeleteTestDefinitionCommand : IRequest
{
    public string Id { get; set; }
}