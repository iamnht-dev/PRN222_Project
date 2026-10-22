using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging.EventLog;
using zCoach.Repositories.ThaiNH;
using zCoach.Repositories.ThaiNH.DBContext;
using zCoach.Services.ThaiNH;
using zCoach.WorkerService.ThaiNH;

// Services can start in System32; resolve settings beside the executable.
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// Keep workstation credentials out of Git; environment and command line still take priority.
builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = builder.Configuration["WindowsService:ServiceName"] ?? "ThaiNH_SE1905_WS";
});
// F5/console demos should not need permission to register a Windows Event Log source.
if (OperatingSystem.IsWindows() && !WindowsServiceHelpers.IsWindowsService())
{
    builder.Logging.AddFilter<EventLogLoggerProvider>(_ => false);
}
builder.Services.AddSingleton<CoachSnapshotWriter>();

builder.Services.AddDbContext<PRN222Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CoachThaiNhRepository>();
builder.Services.AddScoped<SpecializationThaiNhRepository>();
builder.Services.AddScoped<SystemUserAccountRepository>();
builder.Services.AddScoped<ICoachThaiNhService, CoachThaiNhService>();
builder.Services.AddScoped<ISpecializationThaiNhService, SpecializationThaiNhService>();
builder.Services.AddScoped<ICoachMaintenanceService, CoachMaintenanceService>();

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection(WorkerOptions.SectionName));
builder.Services.AddHostedService<CoachWorker>();

var host = builder.Build();
host.Run();
