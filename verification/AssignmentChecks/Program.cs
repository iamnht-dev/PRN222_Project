using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using zCoach.Entities.ThaiNH.Models;
using zCoach.Repositories.ThaiNH;
using zCoach.Repositories.ThaiNH.DBContext;
using zCoach.Services.ThaiNH;

var settingsPath = Path.GetFullPath(args[0]);
var config = new ConfigurationBuilder()
    .SetBasePath(Path.GetDirectoryName(settingsPath)!)
    .AddJsonFile(Path.GetFileName(settingsPath))
    .AddJsonFile("appsettings.Local.json", optional: true).Build();
var options = new DbContextOptionsBuilder<PRN222Context>()
    .UseSqlServer(config.GetConnectionString("DefaultConnection")).Options;
await using var db = new PRN222Context(options);
var repository = new CoachThaiNhRepository(db);
var specializations = new SpecializationThaiNhRepository(db);
var service = new CoachThaiNhService(repository, specializations);
var maintenance = new CoachMaintenanceService(repository);
var baseline = await service.GetAllAsync();
var specialization = (await specializations.GetAllAsync()).First();
var originalName = specialization.SpecializationName;
var testName = "ASM_CHECK_" + Guid.NewGuid().ToString("N");
if (args.Length > 2 && args[1] == "--asm02-signalr")
{
    await RazorSignalRChecks.RunAsync(db, service, args[2]);
    return;
}
await using (var transaction = await db.Database.BeginTransactionAsync())
{
    try
    {
        var coach = await service.CreateAsync(new CoachThaiNh
        {
            FullName = testName, Email = "check@example.com", Phone = "0901234567",
            DateOfBirth = new DateOnly(1995, 1, 2), Gender = "Male", Address = "Test address",
            ImageUrl = "/images/check.png", Experience = "5 years", Certification = "Test certificate",
            Salary = 12000000m, HireDate = DateTime.Today, Status = 1,
            SpecializationThaiNhid = specialization.SpecializationThaiNhid,
            SpecializationThaiNh = specialization, IsActive = true
        });
        Require(coach.CoachThaiNhid > 0, "Create generates ID");
        var found = await service.GetByIdAsync(coach.CoachThaiNhid);
        Require(found?.SpecializationThaiNh?.SpecializationName == originalName, "Read includes specialization");
        Require(found!.DateOfBirth == coach.DateOfBirth && found.Certification == coach.Certification,
            "Coach fields survive create/read");
        Require((await service.SearchAsync(" " + testName + " ", "check@", "0901")).Count == 1,
            "Three search criteria combine with AND and trim input");
        Require((await service.SearchAsync(testName, "no-match", null)).Count == 0, "Search rejects mismatched criterion");
        coach.Salary = 15000000m;
        coach.SpecializationThaiNh = new SpecializationThaiNh
        {
            SpecializationThaiNhid = specialization.SpecializationThaiNhid,
            SpecializationName = "Must not overwrite lookup"
        };
        Require(await service.UpdateAsync(coach), "First update succeeds");
        coach.Certification = "Updated certificate";
        Require(await service.UpdateAsync(coach), "Repeated update in same context succeeds");
        found = await service.GetByIdAsync(coach.CoachThaiNhid);
        Require(found!.Salary == 15000000m && found.Certification == "Updated certificate", "Updated values persist");
        Require((await specializations.GetByIdAsync(specialization.SpecializationThaiNhid))!.SpecializationName == originalName,
            "Updating Coach preserves specialization data");
        var summary = await maintenance.GetSummaryAsync();
        Require(summary.TotalCoaches == baseline.Count + 1 && summary.SpecializationCount > 0, "Summary reads real SQL data");
        coach.Status = 0;
        await service.UpdateAsync(coach);
        Require(await maintenance.DeactivateStoppedAsync() >= 1, "Maintenance finds stopped active Coaches");
        Require((await service.GetByIdAsync(coach.CoachThaiNhid))!.IsActive == false, "Maintenance deactivates stopped Coach");
        Require(await maintenance.DeactivateStoppedAsync() == 0, "Maintenance is idempotent");
        Require(await service.DeleteAsync(coach.CoachThaiNhid), "Delete succeeds");
        Require(await service.GetByIdAsync(coach.CoachThaiNhid) is null, "Deleted Coach no longer exists");
        Require(!await service.DeleteAsync(coach.CoachThaiNhid), "Repeated delete returns false");
        try { await service.CreateAsync(new CoachThaiNh()); throw new Exception("Missing validation"); }
        catch (ValidationException) { Console.WriteLine("PASS: Required full name validated in service"); }
        try
        {
            await service.CreateAsync(new CoachThaiNh { FullName = testName, SpecializationThaiNhid = int.MaxValue });
            throw new Exception("Missing foreign key validation");
        }
        catch (ValidationException) { Console.WriteLine("PASS: Invalid specialization rejected"); }
    }
    finally { await transaction.RollbackAsync(); db.ChangeTracker.Clear(); }
}
Require((await service.GetAllAsync()).Count == baseline.Count, "Transaction rollback preserves original Coach count");

if (args.Length > 1)
{
    var origin = args[1];
    var cookies = new CookieContainer();
    using var handler = new HttpClientHandler { CookieContainer = cookies, AllowAutoRedirect = false };
    using var client = new HttpClient(handler) { BaseAddress = new Uri(origin) };
    var unauth = await client.GetAsync("/CoachThaiNhs");
    Require(unauth.StatusCode == HttpStatusCode.Redirect && unauth.Headers.Location!.ToString().Contains("/Account/Login"),
        "Razor CRUD requires login");
    var account = await db.SystemUserAccounts.FirstAsync(x => x.IsActive);
    var loginHtml = await client.GetStringAsync("/Account/Login");
    var token = Token(loginHtml);
    var loggedIn = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string,string>
    {
        ["Input.UserName"] = account.UserName, ["Input.Password"] = account.Password,
        ["__RequestVerificationToken"] = token
    }));
    Require(loggedIn.StatusCode == HttpStatusCode.Redirect, "Valid local account can log in");
    Require((await client.GetAsync("/CoachThaiNhs")).IsSuccessStatusCode, "Authenticated Razor CRUD list loads");
    foreach (var page in new[] { "/CoachThaiNhs/Create", $"/CoachThaiNhs/Edit?id={baseline.First().CoachThaiNhid}",
        $"/CoachThaiNhs/Details?id={baseline.First().CoachThaiNhid}", $"/CoachThaiNhs/Delete?id={baseline.First().CoachThaiNhid}" })
        Require((await client.GetAsync(page)).IsSuccessStatusCode, "Razor page loads: " + page.Split('?')[0]);
    Require((await client.PostAsync("/CoachThaiNhs/Create",new FormUrlEncodedContent(new Dictionary<string,string>()))).StatusCode == HttpStatusCode.BadRequest,
        "Razor rejects POST without antiforgery token");
    var formHtml = await client.GetStringAsync("/CoachThaiNhs/Create");
    var invalid = await client.PostAsync("/CoachThaiNhs/Create",new FormUrlEncodedContent(new Dictionary<string,string>
    { ["__RequestVerificationToken"]=Token(formHtml), ["Coach.FullName"]="", ["Coach.IsActive"]="true" }));
    Require(invalid.IsSuccessStatusCode && (await invalid.Content.ReadAsStringAsync()).Contains("field is required"),
        "Razor redisplays invalid form with validation errors");
    await CheckFormCrudAsync(client, service, "/CoachThaiNhs", "Coach.", testName + "_Razor");
    var logoutHtml = await client.GetStringAsync("/Account/Logout");
    await client.PostAsync("/Account/Logout",new FormUrlEncodedContent(new Dictionary<string,string>{["__RequestVerificationToken"]=Token(logoutHtml)}));
    Require((await client.GetAsync("/CoachThaiNhs")).StatusCode == HttpStatusCode.Redirect, "Logout clears authenticated session");
}
Console.WriteLine("All assignment checks passed.");
Console.WriteLine("All requested integration checks passed.");
static void Require(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name);
}
static string Token(string html) => WebUtility.HtmlDecode(Regex.Match(html,
    "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);

static async Task CheckFormCrudAsync(HttpClient client, ICoachThaiNhService service, string route, string prefix, string testName)
{
    var id = 0;
    try
    {
        var createHtml = await client.GetStringAsync(route + "/Create");
        var data = new Dictionary<string,string>
        {
            ["__RequestVerificationToken"] = Token(createHtml),
            [prefix+"FullName"] = testName, [prefix+"Email"] = "form@example.com",
            [prefix+"Phone"] = "0901234567", [prefix+"Salary"] = "12000000",
            [prefix+"DateOfBirth"] = "1995-01-02", [prefix+"IsActive"] = "true",
            [prefix+"Status"] = "1"
        };
        var response = await client.PostAsync(route + "/Create", new FormUrlEncodedContent(data));
        Require(response.StatusCode == HttpStatusCode.Redirect, route + " form Create redirects after save");
        var saved = (await service.SearchAsync(testName, "form@example.com", null)).Single();
        id = saved.CoachThaiNhid;
        Require(saved.DateOfBirth == new DateOnly(1995,1,2), route + " form persists DateOnly");
        var editPath = prefix.Length == 0 ? route + "/Edit/" + id : route + "/Edit?id=" + id;
        data["__RequestVerificationToken"] = Token(await client.GetStringAsync(editPath));
        data[prefix+"CoachThaiNhid"] = id.ToString();
        data[prefix+"Salary"] = "15000000";
        response = await client.PostAsync(editPath,new FormUrlEncodedContent(data));
        Require(response.StatusCode == HttpStatusCode.Redirect && (await service.GetByIdAsync(id))!.Salary == 15000000m,
            route + " form Edit persists salary");
        var deletePath = prefix.Length == 0 ? route + "/Delete/" + id : route + "/Delete?id=" + id;
        response = await client.PostAsync(deletePath,new FormUrlEncodedContent(new Dictionary<string,string>
        { ["__RequestVerificationToken"] = Token(await client.GetStringAsync(deletePath)), ["id"] = id.ToString() }));
        Require(response.StatusCode == HttpStatusCode.Redirect && await service.GetByIdAsync(id) is null,
            route + " form Delete removes test Coach");
    }
    finally
    {
        if(id > 0) await service.DeleteAsync(id);
        // If a failed response saved an item before redirecting, clean up only this unique test name.
        foreach(var item in await service.SearchAsync(testName,"form@example.com",null))
            await service.DeleteAsync(item.CoachThaiNhid);
    }
}
