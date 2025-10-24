using Ayjet.Evaluation.Center.Domain.Common;

namespace Ayjet.Evaluation.Center.Domain.Entities;

public class AnswerOption : BaseEntity<int>
{
    public bool IsCorrect { get; set; } // Bu seçeneğin doğru cevap olup olmadığını belirtir.

    // Foreign Key & Navigation Property to the question
    public int MultipleChoiceQuestionId { get; set; }
    public MultipleChoiceQuestion MultipleChoiceQuestion { get; set; }
    
    // Navigation Property
    public ICollection<AnswerOptionTranslation> Translations { get; set; } = new List<AnswerOptionTranslation>();
}