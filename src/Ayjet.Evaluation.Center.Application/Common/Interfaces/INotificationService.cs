namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendScoreUpdateNotificationAsync(string message, string assignmentId);
}
