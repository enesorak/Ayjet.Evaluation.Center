using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;

namespace Ayjet.Evaluation.Center.Application.Features.PsychometricQuestions.Queries.GetById;

public class ScaleMappingDto
{
    public int ScaleId { get; set; }
    public string ScaleName { get; set; } = string.Empty;
    public ScoringDirection ScoringDirection { get; set; }
    public Gender? RequiredGender { get; set; }
}