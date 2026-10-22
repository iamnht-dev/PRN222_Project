using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using zCoach.MVCWebApp.ThaiNH.Models;
using zCoach.Services.ThaiNH;

namespace zCoach.MVCWebApp.ThaiNH.Controllers;

public class AccountController : Controller
{
    private readonly ISystemUserAccountService _accounts;

    public AccountController(ISystemUserAccountService accounts) => _accounts = accounts;

    public IActionResult Login() => View(new LoginRequest());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid) return View(request);

        var account = await _accounts.GetUserAccountAsync(request.UserName, request.Password);
        if (account is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(request);
        }

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.UserAccountId.ToString()),
            new Claim(ClaimTypes.Name, account.UserName),
            new Claim(ClaimTypes.Role, account.RoleId.ToString())
        }, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return RedirectToAction("Index", "Coaches");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public IActionResult Forbidden() => View();
}
