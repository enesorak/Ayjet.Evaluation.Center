using Ayjet.Evaluation.Center.Domain.Common;

namespace Ayjet.Evaluation.Center.Domain.Entities;

public class PsychometricQuestionTranslation : BaseEntityWithGuid
{
    public string Text { get; set; }
    public string Language { get; set; }

    // Foreign Key & Navigation Property to the main question
    public int PsychometricQuestionId { get; set; }
    public PsychometricQuestion PsychometricQuestion { get; set; }
}