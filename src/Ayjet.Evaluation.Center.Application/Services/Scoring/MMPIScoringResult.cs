namespace Ayjet.Evaluation.Center.Application.Services.Scoring;

public class MMPIScoringResult
{
    public Dictionary<string, int> RawScores { get; set; } = new();
    public Dictionary<string, double> CorrectedScores { get; set; } = new();
    public Dictionary<string, int> TScores { get; set; } = new();
}