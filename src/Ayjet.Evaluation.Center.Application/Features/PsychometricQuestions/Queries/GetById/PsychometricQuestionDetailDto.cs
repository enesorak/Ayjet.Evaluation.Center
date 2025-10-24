namespace Ayjet.Evaluation.Center.Application.Features.PsychometricQuestions.Queries.GetById;


public class PsychometricQuestionDetailDto
{
    public int Id { get; set; }
    public int DisplayOrder { get; set; }
    public string? QuestionCode { get; set; }
    public bool IsActive { get; set; }
    public List<TranslationDto> Translations { get; set; } = new();
    public List<ScaleMappingDto> ScaleMappings { get; set; } = new();
}