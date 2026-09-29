using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using zCoach.Services.ThaiNH;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace zCoach.MVCWebApp.ThaiNH.Controllers
{
    public class AccountController : Controller
    {
        private readonly ISystemUserAccountService _userAccountService;

        public AccountController(ISystemUserAccountService systemUserAccountService) => _userAccountService = systemUserAccountService; 

        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Models.LoginRequest request)
        {
            try
            {
                var userAccount = await _userAccountService.GetUserAccount(request.UserName, request.Password);

                if (userAccount != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userAccount.UserName),
                        new Claim(ClaimTypes.Role, userAccount.RoleId.ToString())
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    Response.Cookies.Append("UserName", userAccount.FullName);
                    Response.Cookies.Append("Role", userAccount.RoleId.ToString());

                    return RedirectToAction("Index", "CoachThaiNh");
                }                
            }
            catch (Exception)
            {
            }

            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ModelState.AddModelError("", "Login failure");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("UserName");
            Response.Cookies.Delete("Role");
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> Forbidden()
        {
            return View();
        }
    }
}
