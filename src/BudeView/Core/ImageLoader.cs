using Avalonia.Media.Imaging;

namespace BudeView.Core;

public sealed record ImageLoadResult(Bitmap Bitmap, bool FromCache);

public sealed class ImageLoader(ImageCache cache, TraceWriter trace)
{
    public async Task<ImageLoadResult> LoadAsync(
        string path,
        bool isPreload,
        CancellationToken cancellationToken)
    {
        if (cache.TryGet(path, out var cached))
        {
            trace.Write(
                "cache_hit",
                Path.GetFileName(path),
                new Dictionary<string, string> { ["preload"] = isPreload.ToString() });

            return new ImageLoadResult(cached, FromCache: true);
        }

        trace.Write(
            "cache_miss",
            Path.GetFileName(path),
            new Dictionary<string, string> { ["preload"] = isPreload.ToString() });

        trace.Write(
            "decode_queued",
            Path.GetFileName(path),
            new Dictionary<string, string> { ["preload"] = isPreload.ToString() });

        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        Bitmap? bitmap = null;

        try
        {
            trace.Write(
                "decode_start",
                Path.GetFileName(path),
                new Dictionary<string, string> { ["preload"] = isPreload.ToString() });

            bitmap = await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var stream = new MemoryStream(bytes, writable: false);
                return new Bitmap(stream);
            }, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            trace.Write(
                "decode_end",
                Path.GetFileName(path),
                new Dictionary<string, string>
                {
                    ["preload"] = isPreload.ToString(),
                    ["width"] = bitmap.PixelSize.Width.ToString(),
                    ["height"] = bitmap.PixelSize.Height.ToString()
                });

            return new ImageLoadResult(bitmap, FromCache: false);
        }
        catch
        {
            bitmap?.Dispose();
            throw;
        }
    }
}
