using Ayjet.Evaluation.Center.Application.Services.Scoring; // MMPIScoringResult için

namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Models;

// MMPI raporu için özelleşmiş model
public class MmpiReportModel : BaseReportModel
{
    public required MMPIScoringResult Scores { get; set; } // Ham, Düzeltmeli, T Skorları
    // MMPI'ye özgü başka veriler gerekirse buraya eklenebilir
}