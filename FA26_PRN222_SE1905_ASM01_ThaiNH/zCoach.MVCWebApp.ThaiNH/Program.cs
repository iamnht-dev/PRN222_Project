using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

using zCoach.Repositories.ThaiNH;
using zCoach.Repositories.ThaiNH.DBContext;
using zCoach.Services.ThaiNH;

var builder = WebApplication.CreateBuilder(args);

// Keep workstation credentials out of Git; environment and command line still take priority.
builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "zCoach.Mvc.Session";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Forbidden";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddDbContext<PRN222Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CoachThaiNhRepository>();
builder.Services.AddScoped<SpecializationThaiNhRepository>();
builder.Services.AddScoped<SystemUserAccountRepository>();
builder.Services.AddScoped<ICoachThaiNhService, CoachThaiNhService>();
builder.Services.AddScoped<ISpecializationThaiNhService, SpecializationThaiNhService>();
builder.Services.AddScoped<ISystemUserAccountService, SystemUserAccountService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapControllerRoute(name: "legacyCoach", pattern: "CoachThaiNh/{action=Index}/{id?}", defaults: new { controller = "Coaches" });



app.Run();
