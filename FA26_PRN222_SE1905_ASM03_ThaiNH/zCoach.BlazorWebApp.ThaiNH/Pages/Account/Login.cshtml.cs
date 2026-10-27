using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using zCoach.Services.ThaiNH;

namespace zCoach.BlazorWebApp.ThaiNH.Pages.Account;

[AllowAnonymous]
public class LoginModel(ISystemUserAccountService accounts) : PageModel
{
    [BindProperty, Required]
    public string UserName { get; set; } = string.Empty;

    [BindProperty, Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet()
        => User.Identity?.IsAuthenticated == true ? LocalRedirect(SafeReturnUrl()) : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var account = await accounts.GetUserAccountAsync(UserName, Password);
        if (account is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.UserAccountId.ToString()),
            new Claim(ClaimTypes.Name, account.UserName),
            new Claim(ClaimTypes.Role, account.RoleId.ToString())
        }, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return LocalRedirect(SafeReturnUrl());
    }

    private string SafeReturnUrl() => Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/coaches";
}
