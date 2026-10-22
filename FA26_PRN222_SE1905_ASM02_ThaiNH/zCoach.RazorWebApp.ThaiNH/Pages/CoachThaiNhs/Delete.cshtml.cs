using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using zCoach.Entities.ThaiNH.Models;
using zCoach.Services.ThaiNH;

namespace zCoach.RazorWebApp.ThaiNH.Pages.CoachThaiNhs;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly ICoachThaiNhService _coachService;

    public DeleteModel(ICoachThaiNhService coachService)
    {
        _coachService = coachService;
    }

    public CoachThaiNh Coach { get; set; } = new();

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
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var deleted = await _coachService.DeleteAsync(id);
        TempData[deleted ? "Success" : "Error"] = deleted
            ? $"Đã xoá Coach #{id}."
            : $"Không tìm thấy Coach #{id}.";

        return RedirectToPage("./Index");
    }
}
