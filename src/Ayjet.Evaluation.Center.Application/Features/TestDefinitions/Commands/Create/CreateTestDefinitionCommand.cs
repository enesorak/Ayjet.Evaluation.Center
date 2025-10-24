using System.ComponentModel.DataAnnotations;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Commands.Create;

// Bu komut, IRequest<string> arayüzünü uygular.
// Anlamı: "Bu komut işlendiğinde geriye string tipinde bir sonuç (yeni testin ID'si) dönecek."
public class CreateTestDefinitionCommand : IRequest<string>
{
    [Required] public string Title { get; set; } = null!; // Nullable kalabilir, [Required] boş/null olmamasını garanti eder.

    public string? Description { get; set; }
    public TestType Type { get; set; }
    public int? DefaultTimeLimitInMinutes { get; set; }
    public int? DefaultQuestionCount { get; set; }
    public int? PassingScore { get; set; }
    
}