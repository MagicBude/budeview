namespace BudeView.Core;

public sealed class ImagePreloader : IDisposable
{
    private readonly ImageLoader _loader;
    private readonly ImageCache _cache;
    private readonly TraceWriter _trace;

    private CancellationTokenSource? _cancellation;

    public ImagePreloader(
        ImageLoader loader,
        ImageCache cache,
        TraceWriter trace)
    {
        _loader = loader;
        _cache = cache;
        _trace = trace;
    }

    public void Start(
        DirectoryImageContext context,
        string currentPath,
        int directionHint)
    {
        Cancel();

        _cancellation = new CancellationTokenSource();
        _ = RunAsync(
            context,
            currentPath,
            directionHint,
            _cancellation.Token);
    }

    public void Cancel()
    {
        var cancellation = Interlocked.Exchange(ref _cancellation, null);
        if (cancellation is null)
        {
            return;
        }

        cancellation.Cancel();
        cancellation.Dispose();
    }

    private async Task RunAsync(
        DirectoryImageContext context,
        string currentPath,
        int directionHint,
        CancellationToken cancellationToken)
    {
        try
        {
            foreach (var offset in BuildOffsets(directionHint))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var target = context.GetRelative(currentPath, offset);
                if (target is null)
                {
                    continue;
                }

                _trace.Write(
                    "preload_queued",
                    Path.GetFileName(target),
                    new Dictionary<string, string>
                    {
                        ["offset"] = offset.ToString()
                    });

                var load = await _loader.LoadAsync(
                    target,
                    isPreload: true,
                    cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (!load.FromCache)
                {
                    var retained = _cache.TryStorePreload(target, load.Bitmap);

                    _trace.Write(
                        retained ? "preload_ready" : "preload_discarded",
                        Path.GetFileName(target),
                        new Dictionary<string, string>
                        {
                            ["offset"] = offset.ToString()
                        });
                }
                else
                {
                    _trace.Write(
                        "preload_ready",
                        Path.GetFileName(target),
                        new Dictionary<string, string>
                        {
                            ["offset"] = offset.ToString(),
                            ["source"] = "cache"
                        });
                }
            }
        }
        catch (OperationCanceledException)
        {
            _trace.Write("preload_cancelled");
        }
        catch (Exception ex)
        {
            _trace.Write(
                "preload_failed",
                details: new Dictionary<string, string>
                {
                    ["error"] = ex.GetType().Name
                });
        }
    }

    private static int[] BuildOffsets(int directionHint)
    {
        if (directionHint > 0)
        {
            return [1, 2, -1, -2];
        }

        if (directionHint < 0)
        {
            return [-1, -2, 1, 2];
        }

        return [1, -1, 2, -2];
    }

    public void Dispose()
    {
        Cancel();
    }
}
