using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using zCoach.Entities.ThaiNH.Models;
using zCoach.Services.ThaiNH;

namespace zCoach.MVCWebApp.ThaiNH.Hubs;

[Authorize]
public class CoachHub : Hub
{
    private readonly ICoachThaiNhService _coaches;
    private readonly ISpecializationThaiNhService _specializations;

    public CoachHub(ICoachThaiNhService coaches, ISpecializationThaiNhService specializations)
    {
        _coaches = coaches;
        _specializations = specializations;
    }

    public async Task<IEnumerable<object>> GetAll(string? fullName = null, string? email = null, string? phone = null)
        => (await _coaches.SearchAsync(fullName, email, phone)).Select(MapItem);

    public async Task<object?> GetById(int id)
    {
        var coach = await _coaches.GetByIdAsync(id);
        return coach is null ? null : MapItem(coach);
    }

    public async Task<IEnumerable<object>> GetSpecializations()
        => (await _specializations.GetAllAsync()).Select(x => new { x.SpecializationThaiNhid, x.SpecializationName });

    public async Task<int> Create(CoachDto dto)
    {
        var coach = MapDto(dto);
        coach.CoachThaiNhid = 0;
        try
        {
            var created = await _coaches.CreateAsync(coach);
            await BroadcastChangedAsync("created", created.FullName);
            return created.CoachThaiNhid;
        }
        catch (ValidationException ex) { throw new HubException(ex.Message); }
    }

    public async Task<bool> Update(CoachDto dto)
    {
        if (dto.CoachThaiNhid <= 0) throw new HubException("Invalid Coach ID.");
        var coach = MapDto(dto);
        try
        {
            if (!await _coaches.UpdateAsync(coach)) throw new HubException("Coach was not found.");
            await BroadcastChangedAsync("updated", coach.FullName);
            return true;
        }
        catch (ValidationException ex) { throw new HubException(ex.Message); }
    }

    public async Task<bool> Delete(int id)
    {
        var coach = await _coaches.GetByIdAsync(id) ?? throw new HubException("Coach was not found.");
        if (!await _coaches.DeleteAsync(id)) throw new HubException("Coach was not found.");
        await BroadcastChangedAsync("deleted", coach.FullName);
        return true;
    }

    private Task BroadcastChangedAsync(string action, string fullName)
        => Clients.All.SendAsync("CoachChanged", new { Action = action, FullName = fullName, ChangedAt = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy") });

    private static object MapItem(CoachThaiNh coach) => new
    {
        coach.CoachThaiNhid, coach.FullName, coach.Email, coach.Phone,
        DateOfBirth = coach.DateOfBirth?.ToString("yyyy-MM-dd"),
        coach.Gender, coach.Address, coach.ImageUrl, coach.Experience, coach.Certification, coach.Salary,
        HireDate = coach.HireDate?.ToString("yyyy-MM-dd"), coach.Status,
        PublishDate = coach.PublishDate?.ToString("yyyy-MM-dd"), coach.UpdateAt,
        coach.SpecializationThaiNhid, SpecializationName = coach.SpecializationThaiNh?.SpecializationName, coach.IsActive
    };

    private static CoachThaiNh MapDto(CoachDto dto) => new()
    {
        CoachThaiNhid = dto.CoachThaiNhid, FullName = dto.FullName?.Trim() ?? string.Empty,
        Email = dto.Email, Phone = dto.Phone, DateOfBirth = dto.DateOfBirth, Gender = dto.Gender,
        Address = dto.Address, ImageUrl = dto.ImageUrl, Experience = dto.Experience,
        Certification = dto.Certification, Salary = dto.Salary, HireDate = dto.HireDate,
        Status = dto.Status, PublishDate = dto.PublishDate, SpecializationThaiNhid = dto.SpecializationThaiNhid,
        IsActive = dto.IsActive
    };
}

public class CoachDto
{
    public int CoachThaiNhid { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? ImageUrl { get; set; }
    public string? Experience { get; set; }
    public string? Certification { get; set; }
    public decimal? Salary { get; set; }
    public DateTime? HireDate { get; set; }
    public int? Status { get; set; }
    public DateTime? PublishDate { get; set; }
    public int? SpecializationThaiNhid { get; set; }
    public bool IsActive { get; set; } = true;
}
