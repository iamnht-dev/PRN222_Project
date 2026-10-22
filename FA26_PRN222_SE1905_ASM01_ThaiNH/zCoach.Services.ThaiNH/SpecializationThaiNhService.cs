using zCoach.Entities.ThaiNH.Models;
using zCoach.Repositories.ThaiNH;

namespace zCoach.Services.ThaiNH;

public class SpecializationThaiNhService : ISpecializationThaiNhService
{
    private readonly SpecializationThaiNhRepository _repository;

    public SpecializationThaiNhService(SpecializationThaiNhRepository repository)
    {
        _repository = repository;
    }

    public Task<List<SpecializationThaiNh>> GetAllAsync()
        => _repository.GetAllAsync();
}
