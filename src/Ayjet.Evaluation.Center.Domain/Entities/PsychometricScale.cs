using Ayjet.Evaluation.Center.Domain.Common;

namespace Ayjet.Evaluation.Center.Domain.Entities;

public class PsychometricScale : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    
    public ICollection<QuestionScaleMapping> QuestionMappings { get; set; } = new List<QuestionScaleMapping>();
}