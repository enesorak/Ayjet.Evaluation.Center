using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Commands.Update;

public class UpdateTestDefinitionCommand : IRequest
{
    public string Id { get; set; } // Hangi testin güncelleneceği
    public string Title { get; set; }
    public string? Description { get; set; }
    public int? TimeLimitInMinutes { get; set; }
    
    public int? PassingScore{ get; set; }
}