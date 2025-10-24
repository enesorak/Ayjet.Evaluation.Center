using Ayjet.Evaluation.Center.Domain.Enums;

namespace Ayjet.Evaluation.Center.Domain.Entities;
public class QuestionScaleMapping
{
    public int Id { get; set; }
    public int PsychometricQuestionId { get; set; }
    public PsychometricQuestion PsychometricQuestion { get; set; } = null!;
    public int PsychometricScaleId { get; set; }
    public PsychometricScale PsychometricScale { get; set; } = null!;
    public ScoringDirection ScoringDirection { get; set; } // Hangi cevabın puan getirdiği (True/False)
    
    
    public Gender? RequiredGender { get; set; }

}