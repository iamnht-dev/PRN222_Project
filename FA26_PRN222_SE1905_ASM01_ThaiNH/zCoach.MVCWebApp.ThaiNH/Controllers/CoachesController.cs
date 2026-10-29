using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using zCoach.MVCWebApp.ThaiNH.Hubs;
using zCoach.Entities.ThaiNH.Models;
using zCoach.Services.ThaiNH;

namespace zCoach.MVCWebApp.ThaiNH.Controllers;

[Authorize]
public class CoachesController : Controller
{
    private readonly ICoachThaiNhService _coachService;
    private readonly ISpecializationThaiNhService _specializationService;
    private readonly IHubContext<CoachHub> _hub;

    public CoachesController(
        ICoachThaiNhService coachService,
        ISpecializationThaiNhService specializationService,
        IHubContext<CoachHub> hub)
    {
        _coachService = coachService;
        _specializationService = specializationService;
        _hub = hub;
    }

    // GET: /Coaches?fullName=...&email=...&phone=...
    public async Task<IActionResult> Index(string? fullName, string? email, string? phone)
    {
        ViewData["FullName"] = fullName;
        ViewData["Email"] = email;
        ViewData["Phone"] = phone;
        ViewBag.SearchName = fullName;
        ViewBag.SearchEmail = email;
        ViewBag.SearchPhone = phone;

        var items = await _coachService.SearchAsync(fullName, email, phone);
        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _coachService.GetByIdAsync(id);
        return item is null ? NotFound() : View(item);
    }

    public async Task<IActionResult> Create()
    {
        await LoadSpecializationsAsync();
        return View(new CoachThaiNh
        {

            PublishDate = DateTime.Today,
            IsActive = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CoachThaiNh coach)
    {
        RemoveNavigationErrors();

        if (!ModelState.IsValid)
        {
            await LoadSpecializationsAsync(coach.SpecializationThaiNhid);
            return View(coach);
        }

        await _coachService.CreateAsync(coach);
        await BroadcastChangedAsync("created", coach.FullName);
        TempData["Success"] = $"Đã tạo Coach cho \"{coach.FullName}\".";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _coachService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        await LoadSpecializationsAsync(item.SpecializationThaiNhid);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CoachThaiNh coach)
    {
        if (id != coach.CoachThaiNhid)
        {
            return BadRequest();
        }

        RemoveNavigationErrors();

        if (!ModelState.IsValid)
        {
            await LoadSpecializationsAsync(coach.SpecializationThaiNhid);
            return View(coach);
        }

        var updated = await _coachService.UpdateAsync(coach);
        if (!updated)
        {
            return NotFound();
        }

        await BroadcastChangedAsync("updated", coach.FullName);
        TempData["Success"] = $"Đã cập nhật Coach #{coach.CoachThaiNhid}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _coachService.GetByIdAsync(id);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _coachService.DeleteAsync(id);
        if (deleted) await BroadcastChangedAsync("deleted", $"#{id}");
        TempData[deleted ? "Success" : "Error"] = deleted
            ? $"Đã xoá Coach #{id}."
            : $"Không tìm thấy Coach #{id}.";
        return RedirectToAction(nameof(Index));
    }

    private Task BroadcastChangedAsync(string action, string fullName)
        => _hub.Clients.All.SendAsync("CoachChanged", new
        {
            Action = action,
            FullName = fullName,
            ChangedAt = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")
        });

    private async Task LoadSpecializationsAsync(int? selectedId = null)
    {
        var specializations = await _specializationService.GetAllAsync();
        ViewBag.Specializations = new SelectList(
            specializations,
            nameof(SpecializationThaiNh.SpecializationThaiNhid),
            nameof(SpecializationThaiNh.SpecializationName),
            selectedId);
    }

    /// <summary>
    /// Bỏ qua validate của navigation property vì form chỉ post khoá ngoại.
    /// </summary>
    private void RemoveNavigationErrors()
    {
        var keys = ModelState.Keys
            .Where(k => k.StartsWith(nameof(CoachThaiNh.SpecializationThaiNh), StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keys)
        {
            ModelState.Remove(key);
        }
    }
}
