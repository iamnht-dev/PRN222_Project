using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using zCoach.RazorWebApp.ThaiNH.Models;
using zCoach.Services.ThaiNH;

namespace zCoach.RazorWebApp.ThaiNH.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ISystemUserAccountService _userAccountService;

    public LoginModel(ISystemUserAccountService userAccountService)
    {
        _userAccountService = userAccountService;
    }

    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(returnUrl ?? "/CoachThaiNhs");
        }

        Input.ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var account = await _userAccountService.GetUserAccountAsync(Input.UserName, Input.Password);
        if (account is null)
        {
            ModelState.AddModelError(string.Empty, "Sai tài khoản hoặc mật khẩu.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.UserAccountId.ToString()),
            new(ClaimTypes.Name, account.FullName),
            new(ClaimTypes.Email, account.Email),
            new(ClaimTypes.Role, account.RoleId.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false });

        if (!string.IsNullOrWhiteSpace(Input.ReturnUrl) && Url.IsLocalUrl(Input.ReturnUrl))
        {
            return LocalRedirect(Input.ReturnUrl);
        }

        return RedirectToPage("/CoachThaiNhs/Index");
    }
}
