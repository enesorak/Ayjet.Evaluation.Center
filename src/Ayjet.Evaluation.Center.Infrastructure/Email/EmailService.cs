using Ayjet.Evaluation.Center.Application.Common.Interfaces; // Namespace'i kontrol et
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // Loglama için
using System.Net;
using System.Net.Mail;

namespace Ayjet.Evaluation.Center.Infrastructure.Email; // Namespace'i kontrol et

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger; // Loglama ekledik

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger) // Logger'ı constructor'a ekle
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendTestInvitationEmailAsync(string toEmail, string candidateName, string testTitle, string testLink, DateTime expiresAt)
    {
        // Ayarları User Secrets veya appsettings'den oku
        var host = _configuration["SmtpSettings:Host"];
        var portStr = _configuration["SmtpSettings:Port"];
        var username = _configuration["SmtpSettings:Username"];
        var password = _configuration["SmtpSettings:Password"];
        var enableSslStr = _configuration["SmtpSettings:EnableSsl"];
        var senderEmail = _configuration["SmtpSettings:SenderEmail"];
        var senderName = _configuration["SmtpSettings:SenderName"];

        // Ayarların eksik olup olmadığını kontrol et
        if (string.IsNullOrEmpty(host) || !int.TryParse(portStr, out int port) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || !bool.TryParse(enableSslStr, out bool enableSsl) || string.IsNullOrEmpty(senderEmail))
        {
            _logger.LogError("SMTP settings are missing or invalid in configuration. Cannot send email.");
            return; // Ayarlar eksikse e-posta gönderilemez
        }

        try
        {
            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
                // Timeout süresi eklenebilir: client.Timeout = 10000; // 10 saniye
            };

            var fromAddress = new MailAddress(senderEmail, senderName);
            var toAddress = new MailAddress(toEmail, candidateName);

            var subject = $"Ayjet Değerlendirme Daveti: {testTitle}";
            var body = await GetEmailBody(candidateName, testTitle, testLink, expiresAt); // HTML body'yi al

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Body'nin HTML olduğunu belirt
            };

            _logger.LogInformation("Attempting to send SMTP email to {RecipientEmail} via {SmtpHost}:{SmtpPort}", toEmail, host, port);
            await client.SendMailAsync(message); // Asenkron gönderim
            _logger.LogInformation("Email successfully sent to {RecipientEmail}", toEmail);
        }
        catch (SmtpException smtpEx) // SMTP özel hataları
        {
            _logger.LogError(smtpEx, "SMTP Error sending email to {RecipientEmail}: {ErrorMessage}. Check SMTP settings (Host, Port, Credentials, SSL/TLS).", toEmail, smtpEx.Message);
        }
        catch (Exception ex) // Genel hatalar
        {
            _logger.LogError(ex, "General Error sending email to {RecipientEmail}: {ErrorMessage}", toEmail, ex.Message);
        }
    }

    // HTML şablonunu okuyan metot (öncekiyle aynı)
    private async Task<string> GetEmailBody(string candidateName, string testTitle, string testLink, DateTime expiresAt)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Email/Templates/TestInvitationTemplate.html");
        if (!File.Exists(templatePath))
        {
             _logger.LogError("Email template not found at {TemplatePath}", templatePath);
             return $"Merhaba {candidateName}, {testTitle} testine davet edildiniz. Link: {testLink}"; // Fallback metin
        }

        var template = await File.ReadAllTextAsync(templatePath);

        return template
            .Replace("{{candidateName}}", candidateName)
            .Replace("{{testTitle}}", testTitle)
            .Replace("{{testLink}}", testLink)
            .Replace("{{expiresAt}}", expiresAt.ToString("dd MMMM yyyy, HH:mm")); // Formatı kontrol et
    }
}