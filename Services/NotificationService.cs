using System.Net;
using System.Net.Mail;

namespace FantaScommesse.Services;

public class NotificationService : INotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var from = _configuration["Email:From"] ?? "noreply@fantascommesse.it";
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _logger.LogWarning("Email configuration not set. Email not sent to {To}", to);
            return;
        }

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var message = new MailMessage(from, to, subject, body) { IsBodyHtml = true };

        try
        {
            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
        }
    }

    public async Task SendDeadlineReminderAsync(int roundId)
    {
        // TODO: Implement with actual user email fetching and template
        _logger.LogInformation("Deadline reminder for round {RoundId}", roundId);
        await Task.CompletedTask;
    }

    public async Task SendResultsPublishedAsync(int roundId)
    {
        // TODO: Implement with actual user email fetching and template
        _logger.LogInformation("Results published notification for round {RoundId}", roundId);
        await Task.CompletedTask;
    }

    public async Task SendPaymentReminderAsync(long paymentId)
    {
        // TODO: Implement with actual user email fetching and template
        _logger.LogInformation("Payment reminder for payment {PaymentId}", paymentId);
        await Task.CompletedTask;
    }
}
