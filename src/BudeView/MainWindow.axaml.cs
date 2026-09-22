using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using BudeView.Core;
using BudeView.Viewer;

namespace BudeView;

public sealed partial class MainWindow : Window
{
    private readonly StartupOptions _options;
    private readonly TraceWriter _trace;
    private readonly ImageCache _cache;
    private readonly ImageLoader _loader;
    private readonly ImagePreloader _preloader;

    private DirectoryImageContext? _directoryContext;
    private string? _currentPath;
    private CancellationTokenSource? _requestCancellation;
    private long _requestId;
    private WindowState _windowStateBeforeFullScreen = WindowState.Normal;

    public MainWindow()
        : this(new StartupOptions(null, null, false))
    {
    }

    public MainWindow(StartupOptions options)
    {
        _options = options;
        _trace = new TraceWriter(options.TraceFile);

        var cacheBudget = CacheBudgetPolicy.CalculateDefaultBytes();
        _cache = new ImageCache(cacheBudget, _trace);
        _loader = new ImageLoader(_cache, _trace);
        _preloader = new ImagePreloader(_loader, _cache, _trace);

        InitializeComponent();

        Viewer.ViewChanged += OnViewerViewChanged;
        Viewer.ImageRendered += OnViewerImageRendered;

        Opened += OnOpened;
        Closed += OnClosed;
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        _trace.Write("window_created");

        if (!string.IsNullOrWhiteSpace(_options.ImagePath))
        {
            await OpenPathAsync(
                _options.ImagePath!,
                rebuildDirectoryContext: true,
                directionHint: 0);
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _preloader.Dispose();

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
            await OpenPathAsync(
                path,
                rebuildDirectoryContext: true,
                directionHint: 0);
        }
    }

    private async void PreviousClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await NavigateAsync(-1);

    private async void NextClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await NavigateAsync(1);

    private void FitClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => SetViewerMode(ViewerMode.FitWindow);

    private void FitWidthClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => SetViewerMode(ViewerMode.FitWidth);

    private void FitHeightClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => SetViewerMode(ViewerMode.FitHeight);

    private void FillClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => SetViewerMode(ViewerMode.Fill);

    private void ActualSizeClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => SetViewerMode(ViewerMode.ActualSize);

    private void FullScreenClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => ToggleFullScreen();

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
                SetViewerMode(ViewerMode.FitWindow);
                break;

            case Key.W:
                e.Handled = true;
                SetViewerMode(ViewerMode.FitWidth);
                break;

            case Key.H:
                e.Handled = true;
                SetViewerMode(ViewerMode.FitHeight);
                break;

            case Key.D0:
            case Key.NumPad0:
                e.Handled = true;
                SetViewerMode(ViewerMode.Fill);
                break;

            case Key.D1:
            case Key.NumPad1:
                e.Handled = true;
                SetViewerMode(ViewerMode.ActualSize);
                break;

            case Key.F11:
                e.Handled = true;
                ToggleFullScreen();
                break;

            case Key.Escape when WindowState == Avalonia.Controls.WindowState.FullScreen:
                e.Handled = true;
                ToggleFullScreen();
                break;

            case Key.O when e.KeyModifiers.HasFlag(KeyModifiers.Control):
                e.Handled = true;
                OpenClicked(this, new Avalonia.Interactivity.RoutedEventArgs());
                break;
        }
    }

    private void OnViewerViewChanged(object? sender, EventArgs e)
        => UpdateStatus();

    private void OnViewerImageRendered(object? sender, ImageRenderedEventArgs e)
    {
        _trace.Write(
            "image_rendered",
            Path.GetFileName(e.ContentId));

        if (_options.BenchmarkOnce &&
            string.Equals(e.ContentId, _currentPath, StringComparison.OrdinalIgnoreCase))
        {
            Dispatcher.UIThread.Post(
                () => Close(),
                DispatcherPriority.Background);
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

        await OpenPathAsync(
            target,
            rebuildDirectoryContext: false,
            directionHint: Math.Sign(delta));
    }

    private async Task OpenPathAsync(
        string path,
        bool rebuildDirectoryContext,
        int directionHint)
    {
        path = Path.GetFullPath(path);

        if (!File.Exists(path) || !ImageFileTypes.IsSupported(path))
        {
            StatusText.Text = "Unsupported or missing image. V0.3 supports JPEG and PNG.";
            return;
        }

        var requestId = Interlocked.Increment(ref _requestId);

        _preloader.Cancel();

        _requestCancellation?.Cancel();
        _requestCancellation?.Dispose();
        _requestCancellation = new CancellationTokenSource();

        var cancellationToken = _requestCancellation.Token;

        _currentPath = path;
        _cache.Protect(path);

        StatusText.Text = $"Loading {Path.GetFileName(path)}…";
        PositionText.Text = "";

        Task<DirectoryImageContext>? contextTask = null;
        if (rebuildDirectoryContext)
        {
            contextTask = DirectoryImageContext.CreateAsync(
                path,
                _trace,
                cancellationToken);
        }

        try
        {
            var load = await _loader.LoadAsync(
                path,
                isPreload: false,
                cancellationToken);

            if (requestId != Volatile.Read(ref _requestId) ||
                cancellationToken.IsCancellationRequested)
            {
                if (!load.FromCache)
                {
                    load.Bitmap.Dispose();
                }

                return;
            }

            var bitmap = load.FromCache
                ? load.Bitmap
                : _cache.StoreCurrent(path, load.Bitmap);

            Viewer.SetBitmap(
                bitmap,
                contentId: path,
                resetView: true);

            _trace.Write(
                "present_requested",
                Path.GetFileName(path),
                new Dictionary<string, string>
                {
                    ["width"] = bitmap.PixelSize.Width.ToString(),
                    ["height"] = bitmap.PixelSize.Height.ToString(),
                    ["source"] = load.FromCache ? "cache" : "decoded"
                });

            if (contextTask is not null)
            {
                _directoryContext = await contextTask;
            }

            UpdateNavigationState();
            UpdateStatus();

            if (!_options.BenchmarkOnce && _directoryContext is not null)
            {
                _preloader.Start(
                    _directoryContext,
                    path,
                    directionHint);
            }
        }
        catch (OperationCanceledException)
        {
            _trace.Write(
                "request_cancelled",
                Path.GetFileName(path));
        }
        catch (Exception ex)
        {
            StatusText.Text =
                $"Failed to open {Path.GetFileName(path)}: {ex.Message}";

            _trace.Write(
                "open_failed",
                Path.GetFileName(path),
                new Dictionary<string, string>
                {
                    ["error"] = ex.GetType().Name
                });
        }
    }

    private void SetViewerMode(ViewerMode mode)
    {
        Viewer.SetMode(mode);
        UpdateStatus();
    }

    private void ToggleFullScreen()
    {
        if (WindowState == Avalonia.Controls.WindowState.FullScreen)
        {
            WindowState = _windowStateBeforeFullScreen;
            TopBar.IsVisible = true;
            StatusBar.IsVisible = true;
            return;
        }

        _windowStateBeforeFullScreen =
            WindowState == WindowState.Minimized
                ? Avalonia.Controls.WindowState.Normal
                : WindowState;

        TopBar.IsVisible = false;
        StatusBar.IsVisible = false;
        WindowState = Avalonia.Controls.WindowState.FullScreen;
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
        NextButton.IsEnabled =
            index >= 0 &&
            index < _directoryContext.Files.Count - 1;

        PositionText.Text = index >= 0
            ? $"{index + 1} / {_directoryContext.Files.Count}"
            : "";
    }

    private void UpdateStatus()
    {
        if (string.IsNullOrWhiteSpace(_currentPath) ||
            Viewer.Bitmap is null)
        {
            return;
        }

        var info = new FileInfo(_currentPath);
        var sizeMiB = info.Length / 1024d / 1024d;

        var mode = Viewer.Mode switch
        {
            ViewerMode.FitWindow => "Fit",
            ViewerMode.FitWidth => "Fit Width",
            ViewerMode.FitHeight => "Fit Height",
            ViewerMode.Fill => "Fill",
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
