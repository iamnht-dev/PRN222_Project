namespace zCoach.WorkerService.ThaiNH;

public class WorkerOptions
{
    public const string SectionName = "Worker";
    public int IntervalSeconds { get; set; } = 5;
    public string FilePath { get; set; } = "Logs/ThaiNH_SE1905_WS.txt";
    public bool DeactivateStoppedCoaches { get; set; } = false;
}
