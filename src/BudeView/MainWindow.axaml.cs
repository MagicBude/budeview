using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using BudeView.Core;
using BudeView.Viewer;

namespace BudeView;

public sealed partial class MainWindow : Window
{
    private const long CacheBudgetBytes = 256L * 1024L * 1024L;

    private readonly StartupOptions _options;
    private readonly TraceWriter _trace;
    private readonly ImageCache _cache;
    private readonly ImageLoader _loader;

    private DirectoryImageContext? _directoryContext;
    private string? _currentPath;
    private CancellationTokenSource? _requestCancellation;
    private long _requestId;

    public MainWindow()
        : this(new StartupOptions(null, null))
    {
    }

    public MainWindow(StartupOptions options)
    {
        _options = options;
        _trace = new TraceWriter(options.TraceFile);
        _cache = new ImageCache(CacheBudgetBytes);
        _loader = new ImageLoader(_cache, _trace);

        InitializeComponent();

        Opened += OnOpened;
        Closed += OnClosed;
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        _trace.Write("window_created");

        if (!string.IsNullOrWhiteSpace(_options.ImagePath))
        {
            await OpenPathAsync(_options.ImagePath!, rebuildDirectoryContext: true);
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _requestCancellation?.Cancel();
        _requestCancellation?.Dispose();
        _cache.Dispose();
        _trace.Dispose();
    }

    private async void OpenClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open image",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("JPEG / PNG")
                {
                    Patterns = ["*.jpg", "*.jpeg", "*.png"]
                }
            ]
        });

        var path = files.FirstOrDefault()?.Path.LocalPath;
        if (!string.IsNullOrWhiteSpace(path))
        {
            await OpenPathAsync(path, rebuildDirectoryContext: true);
        }
    }

    private async void PreviousClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await NavigateAsync(-1);

    private async void NextClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await NavigateAsync(1);

    private void FitClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Viewer.SetMode(ViewerMode.FitWindow);
        UpdateStatus();
    }

    private void ActualSizeClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Viewer.SetMode(ViewerMode.ActualSize);
        UpdateStatus();
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                e.Handled = true;
                await NavigateAsync(-1);
                break;

            case Key.Right:
                e.Handled = true;
                await NavigateAsync(1);
                break;

            case Key.F:
                e.Handled = true;
                Viewer.SetMode(ViewerMode.FitWindow);
                UpdateStatus();
                break;

            case Key.D1:
            case Key.NumPad1:
                e.Handled = true;
                Viewer.SetMode(ViewerMode.ActualSize);
                UpdateStatus();
                break;

            case Key.O when e.KeyModifiers.HasFlag(KeyModifiers.Control):
                e.Handled = true;
                OpenClicked(this, new Avalonia.Interactivity.RoutedEventArgs());
                break;
        }
    }

    private async Task NavigateAsync(int delta)
    {
        if (_directoryContext is null || string.IsNullOrWhiteSpace(_currentPath))
        {
            return;
        }

        var target = _directoryContext.GetRelative(_currentPath, delta);
        if (target is null)
        {
            return;
        }

        await OpenPathAsync(target, rebuildDirectoryContext: false);
    }

    private async Task OpenPathAsync(string path, bool rebuildDirectoryContext)
    {
        path = Path.GetFullPath(path);

        if (!File.Exists(path) || !ImageFileTypes.IsSupported(path))
        {
            StatusText.Text = "Unsupported or missing image. V0.2 supports JPEG and PNG.";
            return;
        }

        var requestId = Interlocked.Increment(ref _requestId);

        _requestCancellation?.Cancel();
        _requestCancellation?.Dispose();
        _requestCancellation = new CancellationTokenSource();
        var cancellationToken = _requestCancellation.Token;

        _currentPath = path;
        StatusText.Text = $"Loading {Path.GetFileName(path)}…";
        PositionText.Text = "";

        Task<DirectoryImageContext>? contextTask = null;
        if (rebuildDirectoryContext)
        {
            contextTask = DirectoryImageContext.CreateAsync(path, _trace, cancellationToken);
        }

        try
        {
            var load = await _loader.LoadAsync(path, cancellationToken);

            if (requestId != Volatile.Read(ref _requestId) || cancellationToken.IsCancellationRequested)
            {
                if (!load.FromCache)
                {
                    load.Bitmap.Dispose();
                }

                return;
            }

            Viewer.SetBitmap(load.Bitmap, resetView: true);
            _cache.Add(path, load.Bitmap, protectedPath: path);

            _trace.Write(
                "image_presented",
                Path.GetFileName(path),
                new Dictionary<string, string>
                {
                    ["width"] = load.Bitmap.PixelSize.Width.ToString(),
                    ["height"] = load.Bitmap.PixelSize.Height.ToString(),
                    ["source"] = load.FromCache ? "cache" : "decoded"
                });

            if (contextTask is not null)
            {
                _directoryContext = await contextTask;
            }

            UpdateNavigationState();
            UpdateStatus();
        }
        catch (OperationCanceledException)
        {
            _trace.Write("request_cancelled", Path.GetFileName(path));
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Failed to open {Path.GetFileName(path)}: {ex.Message}";
            _trace.Write(
                "open_failed",
                Path.GetFileName(path),
                new Dictionary<string, string> { ["error"] = ex.GetType().Name });
        }
    }

    private void UpdateNavigationState()
    {
        if (_directoryContext is null || string.IsNullOrWhiteSpace(_currentPath))
        {
            PreviousButton.IsEnabled = false;
            NextButton.IsEnabled = false;
            PositionText.Text = "";
            return;
        }

        var index = _directoryContext.IndexOf(_currentPath);
        PreviousButton.IsEnabled = index > 0;
        NextButton.IsEnabled = index >= 0 && index < _directoryContext.Files.Count - 1;
        PositionText.Text = index >= 0
            ? $"{index + 1} / {_directoryContext.Files.Count}"
            : "";
    }

    private void UpdateStatus()
    {
        if (string.IsNullOrWhiteSpace(_currentPath) || Viewer.Bitmap is null)
        {
            return;
        }

        var info = new FileInfo(_currentPath);
        var sizeMiB = info.Length / 1024d / 1024d;
        var mode = Viewer.Mode switch
        {
            ViewerMode.FitWindow => "Fit",
            ViewerMode.ActualSize => "100%",
            ViewerMode.ManualZoom => $"{Viewer.ZoomFactor * 100:0}%",
            _ => Viewer.Mode.ToString()
        };

        StatusText.Text =
            $"{Path.GetFileName(_currentPath)}   " +
            $"{Viewer.Bitmap.PixelSize.Width}×{Viewer.Bitmap.PixelSize.Height}   " +
            $"{sizeMiB:0.0} MiB   {mode}";
    }
}
