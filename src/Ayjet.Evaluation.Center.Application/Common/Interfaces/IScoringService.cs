namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;

public interface IScoringService
{
   
    Task CalculateAndSaveScoreAsync(string assignmentId);

}