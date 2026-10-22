using Microsoft.Extensions.Options;
using zCoach.Services.ThaiNH;
namespace zCoach.WorkerService.ThaiNH;
// Each iteration owns a fresh DI scope, like Quang's background worker.
public class CoachWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory; private readonly IOptions<WorkerOptions> _options; private readonly ILogger<CoachWorker> _logger;
    public CoachWorker(IServiceScopeFactory scopeFactory, IOptions<WorkerOptions> options, ILogger<CoachWorker> logger) { _scopeFactory = scopeFactory; _options = options; _logger = logger; }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(5, _options.Value.IntervalSeconds));
        _logger.LogInformation("CoachWorker started. Interval={Interval}, DeactivateStoppedCoaches={Deactivate}", interval, _options.Value.DeactivateStoppedCoaches);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var maintenance = scope.ServiceProvider.GetRequiredService<ICoachMaintenanceService>();
                var changed = _options.Value.DeactivateStoppedCoaches ? await maintenance.DeactivateStoppedAsync() : 0;
                var summary = await maintenance.GetSummaryAsync();
                _logger.LogInformation("Deactivated: {Changed} | Total: {Total} | Active: {Active} | Inactive: {Inactive} | Stopped: {Stopped} | Salary: {Salary:N0} | Active salary: {ActiveSalary:N0} | Specializations: {Specializations}", changed, summary.TotalCoaches, summary.ActiveCoaches, summary.InactiveCoaches, summary.StoppedCoaches, summary.TotalSalary, summary.ActiveSalary, summary.SpecializationCount);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { _logger.LogError(ex, "Coach maintenance iteration failed."); }
            try { await Task.Delay(interval, stoppingToken); } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
        _logger.LogInformation("CoachWorker stopped.");
    }
}