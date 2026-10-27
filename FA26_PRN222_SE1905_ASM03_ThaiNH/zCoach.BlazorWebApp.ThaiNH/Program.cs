using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using zCoach.BlazorWebApp.ThaiNH.Components;
using zCoach.Repositories.ThaiNH;
using zCoach.Repositories.ThaiNH.DBContext;
using zCoach.Services.ThaiNH;

var builder = WebApplication.CreateBuilder(args);

// Keep workstation credentials out of Git; environment and command line still take priority.
builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRazorPages();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "zCoach.Blazor.Session";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Forbidden";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

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
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
