namespace BudeView.Core;

public sealed record StartupOptions(
    string? ImagePath,
    string? TraceFile,
    bool BenchmarkOnce)
{
    public static StartupOptions Parse(IEnumerable<string> arguments)
    {
        string? imagePath = null;
        string? traceFile = null;
        var benchmarkOnce = false;

        using var enumerator = arguments.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var arg = enumerator.Current;

            if (string.Equals(arg, "--trace-file", StringComparison.OrdinalIgnoreCase))
            {
                if (enumerator.MoveNext())
                {
                    traceFile = Path.GetFullPath(enumerator.Current);
                }

                continue;
            }

            if (string.Equals(arg, "--benchmark-once", StringComparison.OrdinalIgnoreCase))
            {
                benchmarkOnce = true;
                continue;
            }

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            imagePath ??= arg;
        }

        return new StartupOptions(
            imagePath is null ? null : Path.GetFullPath(imagePath),
            traceFile,
            benchmarkOnce);
    }
}
