using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using zCoach.Entities.ThaiNH.Models;
using zCoach.Services.ThaiNH;

namespace zCoach.RazorWebApp.ThaiNH.Pages.CoachThaiNhs;

[Authorize]
public class CreateModel : CoachFormPageModel
{
    private readonly ICoachThaiNhService _coachService;
    private readonly ISpecializationThaiNhService _specializationService;

    public CreateModel(
        ICoachThaiNhService coachService,
        ISpecializationThaiNhService specializationService)
    {
        _coachService = coachService;
        _specializationService = specializationService;
    }

    public async Task OnGetAsync()
    {
        Coach = new CoachThaiNh
        {

            PublishDate = DateTime.Today,
            IsActive = true
        };

        await LoadSpecializationsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        RemoveNavigationErrors();

        if (!ModelState.IsValid)
        {
            await LoadSpecializationsAsync();
            return Page();
        }

        await _coachService.CreateAsync(Coach);
        TempData["Success"] = $"Đã tạo Coach cho \"{Coach.FullName}\".";
        return RedirectToPage("./Index");
    }

    private async Task LoadSpecializationsAsync()
    {
        var specializations = await _specializationService.GetAllAsync();
        ViewData["SpecializationThaiNhid"] = new SelectList(
            specializations,
            nameof(SpecializationThaiNh.SpecializationThaiNhid),
            nameof(SpecializationThaiNh.SpecializationName),
            Coach.SpecializationThaiNhid);
    }

    private void RemoveNavigationErrors()
    {
        var keys = ModelState.Keys
            .Where(k => k.Contains("SpecializationThaiNh", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keys)
        {
            ModelState.Remove(key);
        }
    }
}
