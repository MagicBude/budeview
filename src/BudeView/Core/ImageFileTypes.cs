namespace BudeView.Core;

public static class ImageFileTypes
{
    private static readonly HashSet<string> Extensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png"
        };

    public static bool IsSupported(string path)
        => Extensions.Contains(Path.GetExtension(path));
}
