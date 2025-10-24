using Ayjet.Evaluation.Center.Domain.Common;
using Ayjet.Evaluation.Center.Domain.Enums;

namespace Ayjet.Evaluation.Center.Domain.Entities;

public class TestDefinition : BaseEntityWithGuid
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public TestType Type { get; set; }
    public int? DefaultTimeLimitInMinutes { get; set; }
    public int? DefaultQuestionCount { get; set; }
    
    public int? PassingScore { get; set; } // <-- YENİ ÖZELLİK

    public bool IsActive { get; set; } = true;

    // Bu iki koleksiyonun da var olduğundan emin olun
    
    public string Language { get; set; } = "tr-TR"; // Varsayılan dil

    public ICollection<MultipleChoiceQuestion> MultipleChoiceQuestions { get; set; } = new List<MultipleChoiceQuestion>();
    public ICollection<PsychometricQuestion> PsychometricQuestions { get; set; } = new List<PsychometricQuestion>();
}