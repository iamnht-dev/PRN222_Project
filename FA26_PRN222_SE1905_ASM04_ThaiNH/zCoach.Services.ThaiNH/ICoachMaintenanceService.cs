using zCoach.Entities.ThaiNH.Models;
namespace zCoach.Services.ThaiNH;

public interface ICoachMaintenanceService { Task<int> DeactivateStoppedAsync(); Task<CoachSummary> GetSummaryAsync(); }