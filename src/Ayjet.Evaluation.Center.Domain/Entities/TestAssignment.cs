using Ayjet.Evaluation.Center.Domain.Common;
using Ayjet.Evaluation.Center.Domain.Enums;

namespace Ayjet.Evaluation.Center.Domain.Entities;

public class TestAssignment : BaseEntityWithGuid
{
    public string CandidateId { get; set; } = string.Empty;
    public Candidate Candidate { get; set; } = null!;

    public string TestDefinitionId { get; set; } = string.Empty;
    public TestDefinition TestDefinition { get; set; } = null!;

    public TestAssignmentStatus Status { get; set; } = TestAssignmentStatus.Pending;
    public DateTime ExpiresAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // === BU İKİ ALANIN NULLABLE (?) OLMASI KRİTİK ===
    public int? TimeLimitInMinutes { get; set; }
    public int? QuestionCount { get; set; }
    // ============================================

    public string Language { get; set; } = "en-EN";
    
    
    public TestResult? TestResult { get; set; }
    public ICollection<CandidateAnswer> CandidateAnswers { get; set; } = new List<CandidateAnswer>();
    
    public ICollection<AssignmentQuestion> AssignedQuestions { get; set; } = new List<AssignmentQuestion>();


}