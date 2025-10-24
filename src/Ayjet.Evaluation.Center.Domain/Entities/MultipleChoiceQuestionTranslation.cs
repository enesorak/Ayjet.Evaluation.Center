using Ayjet.Evaluation.Center.Domain.Common;

namespace Ayjet.Evaluation.Center.Domain.Entities;

public class MultipleChoiceQuestionTranslation : BaseEntityWithGuid
{
    
    
    public string Text { get; set; }
    public string Language { get; set; } = "en_US";

    // Foreign Key & Navigation Property to the main question
    public int MultipleChoiceQuestionId { get; set; }
    public MultipleChoiceQuestion MultipleChoiceQuestion { get; set; }
}