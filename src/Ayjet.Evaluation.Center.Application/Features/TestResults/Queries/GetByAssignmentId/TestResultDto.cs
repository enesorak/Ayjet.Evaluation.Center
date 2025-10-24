using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList;

namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetByAssignmentId;
public class TestResultDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public CandidateDto Candidate { get; set; } = null!; // <-- Değişti

    public string TestTitle { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public decimal Score { get; set; }
    public string? Details { get; set; }
    public string TestType { get; set; } = string.Empty;
    
    public DateTime? StartedAt { get; set; } // <-- YENİ ALAN

    
    public List<AnswerDetailDto> AnswerDetails { get; set; } = new();
}