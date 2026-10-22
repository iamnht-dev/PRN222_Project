using System.ComponentModel.DataAnnotations;
using zCoach.Entities.ThaiNH.Models;
using zCoach.Repositories.ThaiNH;
namespace zCoach.Services.ThaiNH;

public class CoachThaiNhService : ICoachThaiNhService
{
    private readonly CoachThaiNhRepository _repository;
    private readonly SpecializationThaiNhRepository _specializations;
    public CoachThaiNhService(CoachThaiNhRepository repository, SpecializationThaiNhRepository specializations) { _repository = repository; _specializations = specializations; }
    public Task<List<CoachThaiNh>> GetAllAsync() => _repository.GetAllAsync();
    public Task<CoachThaiNh?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<List<CoachThaiNh>> SearchAsync(string? fullName, string? email, string? phone) => _repository.SearchAsync(fullName, email, phone);
    private async Task ValidateAsync(CoachThaiNh coach)
    {
        Validator.ValidateObject(coach, new ValidationContext(coach), validateAllProperties: true);
        if (coach.SpecializationThaiNhid is int id && await _specializations.GetByIdAsync(id) is null)
            throw new ValidationException("Specialization does not exist.");
    }
    public async Task<CoachThaiNh> CreateAsync(CoachThaiNh coach)
    {
        ArgumentNullException.ThrowIfNull(coach);
        await ValidateAsync(coach);
        coach.CoachThaiNhid = 0; coach.SpecializationThaiNh = null;
        coach.PublishDate ??= DateTime.Today; coach.UpdateAt = DateTime.Now;
        return await _repository.CreateAsync(coach);
    }
    public async Task<bool> UpdateAsync(CoachThaiNh coach)
    {
        ArgumentNullException.ThrowIfNull(coach);
        await ValidateAsync(coach);
        var existing = await _repository.GetByIdAsync(coach.CoachThaiNhid);
        if (existing is null) return false;
        existing.FullName = coach.FullName;
        existing.Email = coach.Email;
        existing.Phone = coach.Phone;
        existing.DateOfBirth = coach.DateOfBirth;
        existing.Gender = coach.Gender;
        existing.Address = coach.Address;
        existing.ImageUrl = coach.ImageUrl;
        existing.Experience = coach.Experience;
        existing.Certification = coach.Certification;
        existing.Salary = coach.Salary;
        existing.HireDate = coach.HireDate;
        existing.Status = coach.Status;
        existing.SpecializationThaiNhid = coach.SpecializationThaiNhid;
        existing.IsActive = coach.IsActive;
        existing.PublishDate = coach.PublishDate ?? existing.PublishDate;
        existing.UpdateAt = DateTime.Now; existing.SpecializationThaiNh = null;
        return await _repository.UpdateAsync(existing);
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return false;
        existing.SpecializationThaiNh = null;
        return await _repository.DeleteAsync(existing);
    }
}
