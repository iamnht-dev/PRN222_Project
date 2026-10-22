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

    public IndexModel(ICoachThaiNhService coachService)
    {
        _coachService = coachService;
    }

    public IList<CoachThaiNh> Coaches { get; set; } = new List<CoachThaiNh>();

    [BindProperty(SupportsGet = true)]
    public string? FullName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Email { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Phone { get; set; }

    public async Task OnGetAsync()
    {
        Coaches = await _coachService.SearchAsync(FullName, Email, Phone);
    }
}
