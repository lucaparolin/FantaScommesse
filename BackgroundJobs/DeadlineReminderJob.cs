using FantaScommesse.Repositories;
using FantaScommesse.Services;

namespace FantaScommesse.BackgroundJobs;

public class DeadlineReminderJob : IHostedService, IDisposable
{
    private readonly ILogger<DeadlineReminderJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private Timer? _timer;

    public DeadlineReminderJob(ILogger<DeadlineReminderJob> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deadline Reminder Job started");
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        _logger.LogInformation("Deadline Reminder Job executing");

        using var scope = _serviceProvider.CreateScope();
        var roundRepo = scope.ServiceProvider.GetRequiredService<IRoundRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        try
        {
            var currentRound = await roundRepo.GetCurrentRoundAsync();
            if (currentRound != null)
            {
                var hoursUntilDeadline = (currentRound.DeadlineUtc - DateTime.UtcNow).TotalHours;

                if (hoursUntilDeadline > 0 && hoursUntilDeadline <= 24)
                {
                    _logger.LogInformation("Sending deadline reminder for round {RoundId}", currentRound.RoundId);
                    await notificationService.SendDeadlineReminderAsync(currentRound.RoundId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Deadline Reminder Job");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deadline Reminder Job stopped");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
