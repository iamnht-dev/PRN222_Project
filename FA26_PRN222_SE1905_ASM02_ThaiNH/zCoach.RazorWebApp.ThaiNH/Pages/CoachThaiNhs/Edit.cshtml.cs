using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using zCoach.Entities.ThaiNH.Models;
using zCoach.Services.ThaiNH;

namespace zCoach.RazorWebApp.ThaiNH.Pages.CoachThaiNhs;

[Authorize]
public class EditModel : CoachFormPageModel
{
    private readonly ICoachThaiNhService _coachService;
    private readonly ISpecializationThaiNhService _specializationService;

    public EditModel(
        ICoachThaiNhService coachService,
        ISpecializationThaiNhService specializationService)
    {
        _coachService = coachService;
        _specializationService = specializationService;
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var item = await _coachService.GetByIdAsync(id.Value);
        if (item is null)
        {
            return NotFound();
        }

        Coach = item;
        await LoadSpecializationsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        RemoveNavigationErrors();

        if (!ModelState.IsValid)
        {
            await LoadSpecializationsAsync();
            return Page();
        }

        var updated = await _coachService.UpdateAsync(Coach);
        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = $"Đã cập nhật Coach #{Coach.CoachThaiNhid}.";
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
