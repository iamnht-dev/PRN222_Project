using zCoach.Entities.ThaiNH.Models;

namespace zCoach.Services.ThaiNH;

public interface ICoachThaiNhService
{
    Task<List<CoachThaiNh>> GetAllAsync();
    Task<CoachThaiNh?> GetByIdAsync(int id);
    Task<List<CoachThaiNh>> SearchAsync(string? fullName, string? email, string? phone);
    Task<CoachThaiNh> CreateAsync(CoachThaiNh coach);
    Task<bool> UpdateAsync(CoachThaiNh coach);
    Task<bool> DeleteAsync(int id);
}
