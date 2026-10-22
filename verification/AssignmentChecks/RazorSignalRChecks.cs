using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using zCoach.Repositories.ThaiNH.DBContext;
using zCoach.Services.ThaiNH;

internal static class RazorSignalRChecks
{
    public static async Task RunAsync(PRN222Context db, ICoachThaiNhService service, string origin)
    {
        var baseline = await service.GetAllAsync();
        using var handler = new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer() };
        using var client = new HttpClient(handler) { BaseAddress = new Uri(origin) };
        var unauthorized = await client.PostAsync("/coachHub/negotiate?negotiateVersion=1", null);
        Check(unauthorized.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Unauthorized, "Anonymous SignalR access is rejected");
        var account = await db.SystemUserAccounts.FirstAsync(x => x.IsActive);
        var html = await client.GetStringAsync("/Account/Login");
        var token = WebUtility.HtmlDecode(Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        var login = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        { ["Input.UserName"] = account.UserName, ["Input.Password"] = account.Password, ["__RequestVerificationToken"] = token }));
        Check(login.StatusCode == HttpStatusCode.Redirect, "Razor login succeeds");
        html = await client.GetStringAsync("/CoachThaiNhs");
        foreach (var control in new[] { "btnSignalRCreate", "btn-sr-edit", "btn-sr-delete", "signalRCoachForm", "srFullName", "srSpecialization" })
            Check(html.Contains(control), "Razor renders control: " + control);
        foreach (var asset in new[] { "/js/coaches-signalr.js", "/lib/signalr/signalr.min.js" })
            Check((await client.GetAsync(asset)).IsSuccessStatusCode, "SignalR asset loads: " + asset);

        using var first = await ConnectAsync(client, handler.CookieContainer);
        using var second = await ConnectAsync(client, handler.CookieContainer);
        Check((await InvokeAsync(first, "GetSpecializations")).GetArrayLength() > 0, "Hub loads Coach specializations");
        var name = "ASM_CHECK_SIGNALR_" + Guid.NewGuid().ToString("N");
        var id = 0;
        try
        {
            var payload = new { fullName = name, email = "signalr@example.com", phone = "0901234567", salary = 12000000m,
                dateOfBirth = "1995-01-02", certification = "Certificate", status = 1, isActive = true };
            id = (await InvokeAsync(first, "Create", payload)).GetInt32();
            Check(id > 0, "SignalR Create persists Coach");
            await ExpectChangeAsync(second, "created", name);
            var read = await InvokeAsync(second, "GetById", id);
            Check(read.GetProperty("dateOfBirth").GetString() == "1995-01-02", "Second client reads Coach fields including DateOnly");
            var edit = new { coachThaiNhid = id, fullName = name, email = "signalr@example.com", phone = "0901234567",
                salary = 15000000m, dateOfBirth = "1995-01-02", certification = "Updated", status = 1, isActive = true };
            Check((await InvokeAsync(first, "Update", edit)).GetBoolean(), "SignalR Update succeeds");
            await ExpectChangeAsync(second, "updated", name);
            Check((await InvokeAsync(first, "Update", edit)).GetBoolean(), "Repeated SignalR Update succeeds");
            await ExpectChangeAsync(second, "updated", name);
            Check((await service.GetByIdAsync(id))!.Salary == 15000000m, "SQL salary reflects SignalR update");
            Check((await InvokeAsync(second, "GetAll", name, "signalr@", "0901")).GetArrayLength() == 1,
                "Hub combines three search criteria");
            await ExpectErrorAsync(first, "Create", new { fullName = "", salary = 1 });
            await ExpectErrorAsync(first, "Update", new { coachThaiNhid = -1, fullName = name });
            await ExpectErrorAsync(first, "Create", new { fullName = name, salary = -1 });
            await ExpectErrorAsync(first, "Create", new { fullName = name, specializationThaiNhid = int.MaxValue });
            Check((await InvokeAsync(first, "Delete", id)).GetBoolean(), "SignalR Delete succeeds");
            await ExpectChangeAsync(second, "deleted", name);
            Check((await InvokeAsync(second, "GetById", id)).ValueKind == JsonValueKind.Null, "Second client observes deleted Coach");
            await ExpectErrorAsync(first, "Delete", id);
        }
        finally
        {
            if (id > 0) await service.DeleteAsync(id);
            foreach (var item in await service.SearchAsync(name, null, null)) await service.DeleteAsync(item.CoachThaiNhid);
        }
        Check((await service.GetAllAsync()).Count == baseline.Count, "Original Coach count preserved; test rows removed");
        Console.WriteLine("ASM02 authorized SignalR checks passed.");
    }

    private static void Check(bool passed, string label)
    {
        if (!passed) throw new Exception("FAIL: " + label);
        Console.WriteLine("PASS: " + label);
    }

    private static async Task<ClientWebSocket> ConnectAsync(HttpClient client, CookieContainer cookies)
    {
        using var negotiation = await client.PostAsync("/coachHub/negotiate?negotiateVersion=1", null);
        negotiation.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await negotiation.Content.ReadAsStringAsync());
        var socket = new ClientWebSocket();
        socket.Options.Cookies = cookies;
        var uri = new UriBuilder(client.BaseAddress!) { Scheme = "ws", Path = "/coachHub",
            Query = "id=" + Uri.EscapeDataString(json.RootElement.GetProperty("connectionToken").GetString()!) }.Uri;
        await socket.ConnectAsync(uri, CancellationToken.None);
        await SendAsync(socket, new { protocol = "json", version = 1 });
        await ReceiveAsync(socket);
        return socket;
    }

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ClientWebSocket, Queue<string>> Frames = new();

    private static async Task<JsonElement> ReceiveAsync(ClientWebSocket socket)
    {
        var queue = Frames.GetValue(socket, _ => new Queue<string>());
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        while (queue.Count == 0)
        {
            var bytes = new byte[1024 * 1024];
            var offset = 0;
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(bytes, offset, bytes.Length - offset), timeout.Token);
                if (result.MessageType == WebSocketMessageType.Close) throw new Exception("Socket closed unexpectedly");
                offset += result.Count;
            } while (!result.EndOfMessage);
            foreach (var frame in Encoding.UTF8.GetString(bytes, 0, offset).Split('\u001e', StringSplitOptions.RemoveEmptyEntries)) queue.Enqueue(frame);
        }
        using var json = JsonDocument.Parse(queue.Dequeue());
        return json.RootElement.Clone();
    }

    private static async Task SendAsync(ClientWebSocket socket, object payload) =>
        await socket.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload) + "\u001e"), WebSocketMessageType.Text, true, CancellationToken.None);

    private static async Task<JsonElement> InvokeAsync(ClientWebSocket socket, string target, params object?[] arguments)
    {
        var id = Guid.NewGuid().ToString("N");
        await SendAsync(socket, new { type = 1, invocationId = id, target, arguments });
        while (true)
        {
            var frame = await ReceiveAsync(socket);
            if (frame.TryGetProperty("type", out var type) && type.GetInt32() == 3 && frame.GetProperty("invocationId").GetString() == id)
            {
                if (frame.TryGetProperty("error", out var error)) throw new HubCheckException(error.GetString());
                return frame.TryGetProperty("result", out var result) ? result.Clone() : JsonSerializer.SerializeToElement<object?>(null);
            }
        }
    }

    private static async Task ExpectChangeAsync(ClientWebSocket socket, string action, string name)
    {
        JsonElement frame;
        do { frame = await ReceiveAsync(socket); } while (frame.GetProperty("type").GetInt32() == 6);
        Check(frame.GetProperty("target").GetString() == "CoachChanged" && frame.GetProperty("arguments")[0].GetProperty("action").GetString() == action &&
            frame.GetProperty("arguments")[0].GetProperty("fullName").GetString() == name, "Second client receives " + action + " notification");
    }

    private static async Task ExpectErrorAsync(ClientWebSocket socket, string target, params object?[] arguments)
    {
        try { await InvokeAsync(socket, target, arguments); }
        catch (HubCheckException) { Console.WriteLine("PASS: Hub rejects invalid " + target); return; }
        throw new Exception("FAIL: Invalid " + target + " accepted");
    }

    private sealed class HubCheckException(string? message) : Exception(message);
}
