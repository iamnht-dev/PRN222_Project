using Microsoft.EntityFrameworkCore;
using zCoach.Entities.ThaiNH.Models;
using zCoach.Repositories.ThaiNH.Base;
using zCoach.Repositories.ThaiNH.DBContext;
namespace zCoach.Repositories.ThaiNH;

public class SpecializationThaiNhRepository : GenericRepository<SpecializationThaiNh>
{
    public SpecializationThaiNhRepository(PRN222Context context) : base(context) { }
    public override Task<List<SpecializationThaiNh>> GetAllAsync() => _context.SpecializationThaiNhs.OrderBy(x => x.SpecializationName).ToListAsync();
}