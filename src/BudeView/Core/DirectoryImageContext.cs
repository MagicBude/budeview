namespace BudeView.Core;

public sealed class DirectoryImageContext
{
    private DirectoryImageContext(IReadOnlyList<string> files)
    {
        Files = files;
    }

    public IReadOnlyList<string> Files { get; }

    public static Task<DirectoryImageContext> CreateAsync(
        string currentPath,
        TraceWriter trace,
        CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var directory = Path.GetDirectoryName(currentPath)
                ?? throw new InvalidOperationException("Image has no parent directory.");

            trace.Write("folder_scan_start", Path.GetFileName(currentPath));

            cancellationToken.ThrowIfCancellationRequested();

            var files = Directory
                .EnumerateFiles(directory)
                .Where(ImageFileTypes.IsSupported)
                .OrderBy(static file => Path.GetFileName(file), NaturalStringComparer.Instance)
                .ToArray();

            cancellationToken.ThrowIfCancellationRequested();

            trace.Write(
                "folder_scan_end",
                Path.GetFileName(currentPath),
                new Dictionary<string, string> { ["count"] = files.Length.ToString() });

            return new DirectoryImageContext(files);
        }, cancellationToken);
    }

    public int IndexOf(string path)
    {
        for (var index = 0; index < Files.Count; index++)
        {
            if (string.Equals(Files[index], path, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    public string? GetRelative(string currentPath, int delta)
    {
        var index = IndexOf(currentPath);
        if (index < 0)
        {
            return null;
        }

        var target = index + delta;
        return target >= 0 && target < Files.Count
            ? Files[target]
            : null;
    }
}
