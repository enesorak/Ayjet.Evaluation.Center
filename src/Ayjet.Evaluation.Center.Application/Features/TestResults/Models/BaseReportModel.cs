using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList; // CandidateDto için

namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Models;

// Tüm raporlar için ortak olabilecek temel model
public abstract class BaseReportModel
{
    public required CandidateDto Candidate { get; set; }
    public required string TestTitle { get; set; }
    public DateTime? CompletedAt { get; set; }
    // Diğer ortak alanlar eklenebilir
}