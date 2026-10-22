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
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public CoachSnapshotWriter(IOptions<WorkerOptions> options) => _options = options.Value;
    public string FilePath => Path.GetFullPath(_options.FilePath, AppContext.BaseDirectory);

    public async Task WriteAsync(List<CoachThaiNh> coaches, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var json = JsonSerializer.Serialize(coaches, JsonOptions);
        await File.AppendAllTextAsync(FilePath, $"{DateTimeOffset.Now:O}: {json}{Environment.NewLine}", Encoding.UTF8, cancellationToken);
    }
}
