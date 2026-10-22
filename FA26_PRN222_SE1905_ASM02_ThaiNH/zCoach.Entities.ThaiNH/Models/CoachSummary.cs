namespace zCoach.Entities.ThaiNH.Models;

public class CoachSummary
{
    public int TotalCoaches { get; set; }
    public int ActiveCoaches { get; set; }
    public int InactiveCoaches { get; set; }
    public int StoppedCoaches { get; set; }
    public decimal TotalSalary { get; set; }
    public decimal ActiveSalary { get; set; }
    public int SpecializationCount { get; set; }
}