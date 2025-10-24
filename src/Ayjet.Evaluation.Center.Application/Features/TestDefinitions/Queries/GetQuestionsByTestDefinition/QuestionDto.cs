namespace Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Queries.GetQuestionsByTestDefinition;

public class QuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty; // <-- YENİ ALAN

    public string TestType { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string? QuestionCode { get; set; } // <-- ADD THIS LINE
    public int DifficultyLevel { get; set; } // <-- YENİ ALAN

    public List<OptionDto>? Options { get; set; }
}