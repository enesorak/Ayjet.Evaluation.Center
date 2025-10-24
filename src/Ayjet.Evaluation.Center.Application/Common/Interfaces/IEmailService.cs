namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendTestInvitationEmailAsync(
        string toEmail,
        string candidateName,
        string testTitle,
        string testLink,
        DateTime expiresAt);
}