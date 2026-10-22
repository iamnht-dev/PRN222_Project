using zCoach.Entities.ThaiNH.Models;
using zCoach.Repositories.ThaiNH;
namespace zCoach.Services.ThaiNH;

public class CoachMaintenanceService : ICoachMaintenanceService
{
    private readonly CoachThaiNhRepository _repository;
    public CoachMaintenanceService(CoachThaiNhRepository repository) => _repository = repository;
    public async Task<int> DeactivateStoppedAsync()
    {
        var items = await _repository.GetStoppedActiveAsync();
        foreach (var item in items) { item.IsActive = false; item.UpdateAt = DateTime.Now; }
        if (items.Count > 0) await _repository.UpdateRangeAsync(items);
        return items.Count;
    }
    public Task<CoachSummary> GetSummaryAsync() => _repository.GetSummaryAsync();
}