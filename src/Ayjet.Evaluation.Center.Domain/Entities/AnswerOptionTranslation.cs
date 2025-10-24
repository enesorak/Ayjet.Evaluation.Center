using Ayjet.Evaluation.Center.Domain.Common;

namespace Ayjet.Evaluation.Center.Domain.Entities;


public class AnswerOptionTranslation : BaseEntityWithGuid
{
    public string Text { get; set; }
    public string Language { get; set; } // Örn: "tr-TR", "en-US"

    // Foreign Key & Navigation Property to the answer option
    public int AnswerOptionId { get; set; }
    public AnswerOption AnswerOption { get; set; }
}