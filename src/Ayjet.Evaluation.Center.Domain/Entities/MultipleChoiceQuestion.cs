using Ayjet.Evaluation.Center.Domain.Common;

namespace Ayjet.Evaluation.Center.Domain.Entities;

public class MultipleChoiceQuestion : BaseEntity<int> ,IQuestion
{
    
    public bool IsActive { get; set; } = true;
    public string? QuestionCode { get; set; } // Kullanıcı dostu, benzersiz bir kod (örn: AYJ-MC-0012)
    public int DisplayOrder { get; set; } 
    public int DifficultyLevel { get; set; } // Örn: 1 (Kolay) - 5 (Zor)

    // Foreign Key & Navigation Property to TestDefinition
    public string TestDefinitionId { get; set; }
    public TestDefinition TestDefinition { get; set; }

    // Navigation Properties
    public ICollection<MultipleChoiceQuestionTranslation> Translations { get; set; } = new List<MultipleChoiceQuestionTranslation>();
    public ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();
    
    public ICollection<AssignmentQuestion> Assignments { get; set; } = new List<AssignmentQuestion>();

}