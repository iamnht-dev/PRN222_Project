using Microsoft.Extensions.Options;
using zCoach.Services.ThaiNH;
namespace zCoach.WorkerService.ThaiNH;
public class CoachWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WorkerOptions _options;
    private readonly CoachSnapshotWriter _writer;
    private readonly ILogger<CoachWorker> _logger;

    public CoachWorker(IServiceScopeFactory scopeFactory, IOptions<WorkerOptions> options,
        CoachSnapshotWriter writer, ILogger<CoachWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _writer = writer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(5, _options.IntervalSeconds));
        _logger.LogInformation("Coach worker started. Interval: {Interval}. Text file: {FilePath}", interval, _writer.FilePath);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var coaches = scope.ServiceProvider.GetRequiredService<ICoachThaiNhService>();
                    var items = await coaches.GetAllAsync();
                    await _writer.WriteAsync(items, stoppingToken);
                    _logger.LogInformation("Appended {Count} Coaches to {FilePath} at {Time}", items.Count, _writer.FilePath, DateTimeOffset.Now);

                    // Optional earlier feature; Lab4's default is read and write a text file.
                    if (_options.DeactivateStoppedCoaches)
                    {
                        var maintenance = scope.ServiceProvider.GetRequiredService<ICoachMaintenanceService>();
                        var changed = await maintenance.DeactivateStoppedAsync();
                        _logger.LogInformation("Deactivated {Count} stopped Coaches.", changed);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex) { _logger.LogError(ex, "Could not read Coaches or append text file. Retrying next interval."); }
                await Task.Delay(interval, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        _logger.LogInformation("CoachWorker stopped.");
    }
}
