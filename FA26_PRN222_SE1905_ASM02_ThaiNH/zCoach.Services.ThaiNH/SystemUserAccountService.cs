using zCoach.Entities.ThaiNH.Models;
using zCoach.Repositories.ThaiNH;

namespace zCoach.Services.ThaiNH;

public class SystemUserAccountService : ISystemUserAccountService
{
    private readonly SystemUserAccountRepository _repository;

    public SystemUserAccountService(SystemUserAccountRepository repository)
    {
        _repository = repository;
    }

    public Task<SystemUserAccount?> GetUserAccountAsync(string userName, string password)
        => _repository.GetUserAccountAsync(userName, password);
}
