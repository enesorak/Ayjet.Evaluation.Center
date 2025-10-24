using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Configuration;

namespace Ayjet.Evaluation.Center.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendTestInvitationEmailAsync(string toEmail, string candidateName, string testTitle, string testLink, DateTime expiresAt)
    {
        var apiKey = _configuration["Mailjet:ApiKey"];
        var apiSecret = _configuration["Mailjet:ApiSecret"];
        var senderEmail = _configuration["Mailjet:SenderEmail"];

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret) || string.IsNullOrEmpty(senderEmail))
        {
            Console.WriteLine("Mailjet API Key/Secret or Sender Email is not configured.");
            // Gerçek bir uygulamada burada ILogger ile loglama yaparız.
            return;
        }

        var client = new MailjetClient(apiKey, apiSecret);
        
        var email = new TransactionalEmailBuilder()
           
            .WithFrom(new SendContact(
                senderEmail,  
                "Ayjet Değerlendirme Merkezi"
            ))
            // ---------------------
            .WithSubject($"Ayjet Değerlendirme Daveti: {testTitle}")
            .WithHtmlPart(await GetEmailBody(candidateName, testTitle, testLink, expiresAt))
            .WithTo(new SendContact(toEmail, candidateName))
            .Build();
        
        var response = await client.SendTransactionalEmailAsync(email);

        if (response.Messages?.Length > 0 && response.Messages[0].Status != "success")
        { 
            var errorInfo = response.Messages[0].Errors;
            // Gerçek bir uygulamada burada ILogger ile loglama yaparız.
            Console.WriteLine($"Mailjet Error: {errorInfo?[0].ErrorMessage}");
        }
        else
        {
            Console.WriteLine($"Email successfully queued via Mailjet to {toEmail}.");
        }
    }

    private async Task<string> GetEmailBody(string candidateName, string testTitle, string testLink, DateTime expiresAt)
    {
        // Assembly.GetExecutingAssembly().Location yerine AppContext.BaseDirectory kullanıyoruz.
        // Bu, uygulamanın çalıştığı ana dizini daha güvenilir bir şekilde verir.
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Email/Templates/TestInvitationTemplate.html");

        var template = await File.ReadAllTextAsync(templatePath);

        return template
            .Replace("{{candidateName}}", candidateName)
            .Replace("{{testTitle}}", testTitle)
            .Replace("{{testLink}}", testLink)
            .Replace("{{expiresAt}}", expiresAt.ToString("dd MMMM yyyy, HH:mm"));
    }
}