namespace FantaScommesse.Services;

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendDeadlineReminderAsync(int roundId);
    Task SendResultsPublishedAsync(int roundId);
    Task SendPaymentReminderAsync(long paymentId);
}
