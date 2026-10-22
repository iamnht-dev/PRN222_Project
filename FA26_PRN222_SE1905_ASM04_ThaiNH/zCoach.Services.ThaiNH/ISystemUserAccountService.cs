using zCoach.Entities.ThaiNH.Models;

namespace zCoach.Services.ThaiNH;

public interface ISystemUserAccountService
{
    Task<SystemUserAccount?> GetUserAccountAsync(string userName, string password);
}
