using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using zCoach.Entities.ThaiNH.Models;

namespace zCoach.RazorWebApp.ThaiNH.Pages.CoachThaiNhs;

/// <summary>
/// Base cho Create/Edit để dùng chung partial _FormFields.
/// </summary>
public abstract class CoachFormPageModel : PageModel
{
    [BindProperty]
    public CoachThaiNh Coach { get; set; } = new();
}
