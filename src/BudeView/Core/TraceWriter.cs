using System.Diagnostics;
using System.Text.Json;

namespace BudeView.Core;

public sealed class TraceWriter : IDisposable
{
    private readonly object _gate = new();
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly string _sessionId =
        $"{Environment.ProcessId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
    private readonly StreamWriter? _writer;

    public TraceWriter(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        _writer = new StreamWriter(
            new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.Read),
            new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
        {
            AutoFlush = true
        };

        Write("process_start");
    }

    public void Write(
        string eventName,
        string? image = null,
        Dictionary<string, string>? details = null)
    {
        if (_writer is null)
        {
            return;
        }

        var traceEvent = new TraceEvent(
            Schema: 1,
            SessionId: _sessionId,
            Event: eventName,
            ElapsedMilliseconds: _stopwatch.Elapsed.TotalMilliseconds,
            Image: image,
            Details: details);

        var line = JsonSerializer.Serialize(
            traceEvent,
            BudeViewJsonContext.Default.TraceEvent);

        lock (_gate)
        {
            _writer.WriteLine(line);
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _writer?.Dispose();
        }
    }
}

public sealed record TraceEvent(
    int Schema,
    string SessionId,
    string Event,
    double ElapsedMilliseconds,
    string? Image,
    Dictionary<string, string>? Details);
