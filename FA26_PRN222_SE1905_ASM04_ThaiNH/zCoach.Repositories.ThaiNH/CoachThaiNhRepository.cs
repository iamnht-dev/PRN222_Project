using Microsoft.EntityFrameworkCore;
using zCoach.Entities.ThaiNH.Models;
using zCoach.Repositories.ThaiNH.Base;
using zCoach.Repositories.ThaiNH.DBContext;
namespace zCoach.Repositories.ThaiNH;

public class CoachThaiNhRepository : GenericRepository<CoachThaiNh>
{
    public CoachThaiNhRepository(PRN222Context context) : base(context) { }
    public override Task<List<CoachThaiNh>> GetAllAsync() => SearchAsync(null, null, null);
    public override Task<CoachThaiNh?> GetByIdAsync(int id) => _context.CoachThaiNhs.Include(x => x.SpecializationThaiNh).FirstOrDefaultAsync(x => x.CoachThaiNhid == id);
    public Task<List<CoachThaiNh>> SearchAsync(string? fullName, string? email, string? phone)
    {
        var query = _context.CoachThaiNhs.Include(x => x.SpecializationThaiNh).AsQueryable();
        if (!string.IsNullOrWhiteSpace(fullName)) { var term = fullName.Trim(); query = query.Where(x => x.FullName.Contains(term)); }
        if (!string.IsNullOrWhiteSpace(email)) { var term = email.Trim(); query = query.Where(x => x.Email != null && x.Email.Contains(term)); }
        if (!string.IsNullOrWhiteSpace(phone)) { var term = phone.Trim(); query = query.Where(x => x.Phone != null && x.Phone.Contains(term)); }
        return query.OrderByDescending(x => x.CoachThaiNhid).ToListAsync();
    }
    public Task<List<CoachThaiNh>> GetStoppedActiveAsync() => _context.CoachThaiNhs.AsTracking().Where(x => x.IsActive && x.Status == 0).ToListAsync();
    public async Task<CoachSummary> GetSummaryAsync() => new()
    {
        TotalCoaches = await _context.CoachThaiNhs.CountAsync(),
        ActiveCoaches = await _context.CoachThaiNhs.CountAsync(x => x.IsActive),
        InactiveCoaches = await _context.CoachThaiNhs.CountAsync(x => !x.IsActive),
        StoppedCoaches = await _context.CoachThaiNhs.CountAsync(x => x.Status == 0),
        TotalSalary = await _context.CoachThaiNhs.SumAsync(x => x.Salary) ?? 0m,
        ActiveSalary = await _context.CoachThaiNhs.Where(x => x.IsActive).SumAsync(x => x.Salary) ?? 0m,
        SpecializationCount = await _context.SpecializationThaiNhs.CountAsync()
    };
}
