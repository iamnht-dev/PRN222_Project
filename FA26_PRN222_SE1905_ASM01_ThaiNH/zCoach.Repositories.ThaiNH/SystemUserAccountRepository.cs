using Microsoft.EntityFrameworkCore;
using zCoach.Entities.ThaiNH.Models;
using zCoach.Repositories.ThaiNH.Base;
using zCoach.Repositories.ThaiNH.DBContext;

namespace zCoach.Repositories.ThaiNH;

public class SystemUserAccountRepository : GenericRepository<SystemUserAccount>
{
    public SystemUserAccountRepository(PRN222Context context) : base(context)
    {
    }

    public async Task<SystemUserAccount?> GetUserAccountAsync(string userName, string password)
    {
        return await _context.SystemUserAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserName == userName &&
                x.Password == password &&
                x.IsActive);
    }
}
