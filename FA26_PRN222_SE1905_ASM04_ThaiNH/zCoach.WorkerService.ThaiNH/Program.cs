using Microsoft.EntityFrameworkCore;
using zCoach.Repositories.ThaiNH;
using zCoach.Repositories.ThaiNH.DBContext;
using zCoach.Services.ThaiNH;
using zCoach.WorkerService.ThaiNH;

var builder = Host.CreateApplicationBuilder(args);

// Keep workstation credentials out of Git; environment and command line still take priority.
builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

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
