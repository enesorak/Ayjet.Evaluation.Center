using Ayjet.Evaluation.Center.Domain.Common;

namespace Ayjet.Evaluation.Center.Domain.Entities;

public class PsychometricQuestion : BaseEntity<int>, IQuestion
{
    public string TestDefinitionId { get; set; }
    public TestDefinition TestDefinition { get; set; } = null!;

   
    
    public bool IsActive { get; set; } = true;  


    public ICollection<PsychometricQuestionTranslation> Translations { get; set; } = new List<PsychometricQuestionTranslation>();
    public ICollection<QuestionScaleMapping> ScaleMappings { get; set; } = new List<QuestionScaleMapping>();
    public int DisplayOrder { get; set; }
}