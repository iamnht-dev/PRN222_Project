namespace zCoach.WorkerService.ThaiNH;

public class WorkerOptions
{
    public const string SectionName = "Worker";
    public int IntervalSeconds { get; set; } = 30;
    public bool DeactivateStoppedCoaches { get; set; } = true;
}