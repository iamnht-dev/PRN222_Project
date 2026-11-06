using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using zCoach.Entities.ThaiNH.Models;
using zCoach.Services.ThaiNH;

namespace zCoach.RazorWebApp.ThaiNH.Pages.CoachThaiNhs;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ICoachThaiNhService _coachService;
    private readonly ISpecializationThaiNhService _specializationService;

    public IndexModel(ICoachThaiNhService coachService, ISpecializationThaiNhService specializationService)
    {
        _coachService = coachService;
        _specializationService = specializationService;
    }

    public IList<CoachThaiNh> Coaches { get; private set; } = new List<CoachThaiNh>();
    public IList<SpecializationThaiNh> Specializations { get; private set; } = new List<SpecializationThaiNh>();

    [BindProperty(SupportsGet = true)]
    public string? FullName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Email { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Phone { get; set; }

    [BindProperty]
    public CoachThaiNh Editor { get; set; } = new();

    public async Task OnGetAsync() => await LoadPageDataAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        RemoveNavigationErrors();
        if (!ModelState.IsValid)
        {
            await LoadPageDataAsync();
            return Page();
        }

        var created = await _coachService.CreateAsync(Editor);
        TempData["Success"] = $"Đã tạo Coach #{created.CoachThaiNhid} cho \"{created.FullName}\".";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        RemoveNavigationErrors();
        if (!ModelState.IsValid)
        {
            await LoadPageDataAsync();
            return Page();
        }

        var updated = await _coachService.UpdateAsync(Editor);
        if (!updated) return NotFound();
        TempData["Success"] = $"Đã cập nhật Coach #{Editor.CoachThaiNhid}.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await _coachService.DeleteAsync(id);
        TempData[deleted ? "Success" : "Error"] = deleted ? $"Đã xoá Coach #{id}." : "Không tìm thấy Coach.";
        return RedirectToPage();
    }

    private async Task LoadPageDataAsync()
    {
        Coaches = await _coachService.SearchAsync(FullName, Email, Phone);
        Specializations = await _specializationService.GetAllAsync();
    }

    private void RemoveNavigationErrors()
    {
        foreach (var key in ModelState.Keys.Where(k => k.Contains("SpecializationThaiNh", StringComparison.OrdinalIgnoreCase)).ToList())
            ModelState.Remove(key);
    }
}
