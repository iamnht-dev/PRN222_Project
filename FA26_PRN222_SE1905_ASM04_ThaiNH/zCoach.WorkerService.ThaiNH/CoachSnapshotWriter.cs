using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using zCoach.Entities.ThaiNH.Models;

namespace zCoach.WorkerService.ThaiNH;

public sealed class CoachSnapshotWriter
{
    private readonly WorkerOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public CoachSnapshotWriter(IOptions<WorkerOptions> options) => _options = options.Value;
    public string FilePath => Path.GetFullPath(_options.FilePath, AppContext.BaseDirectory);

    public async Task WriteAsync(List<CoachThaiNh> coaches, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var json = JsonSerializer.Serialize(coaches, JsonOptions);
        var timestamp = DateTimeOffset.Now;
        var entry = $"{new string('=', 72)}{Environment.NewLine}"
            + $"THAI NH - COACH WINDOWS SERVICE{Environment.NewLine}"
            + $"Thoi gian ghi log: {timestamp:dd/MM/yyyy HH:mm:ss.fff zzz}{Environment.NewLine}"
            + $"So luong Coach: {coaches.Count}{Environment.NewLine}"
            + $"{new string('-', 72)}{Environment.NewLine}"
            + json + Environment.NewLine + Environment.NewLine;
        await File.AppendAllTextAsync(FilePath, entry, Encoding.UTF8, cancellationToken);
    }
}
