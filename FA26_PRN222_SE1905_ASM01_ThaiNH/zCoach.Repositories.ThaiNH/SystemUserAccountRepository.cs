using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using zCoach.Entities.ThaiNH.Models;
using zCoach.Repositories.ThaiNH.Base;
using zCoach.Repositories.ThaiNH.DbContext;

namespace zCoach.Repositories.ThaiNH
{
    public class SystemUserAccountRepository : GenericRepository<SystemUserAccount>
    {
        public SystemUserAccountRepository() => _context ??= new PRN222Context();
        
        public SystemUserAccountRepository(PRN222Context context) => _context = context;

        public async Task<SystemUserAccount> GetUserAccount(string userName, string password)
        {
            return await _context.SystemUserAccounts.FirstOrDefaultAsync(u => u.UserName == userName && u.Password == password && u.IsActive);
        }
    }
}
