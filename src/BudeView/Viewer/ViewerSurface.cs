using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace BudeView.Viewer;

public sealed class ViewerSurface : Control
{
    private static readonly IBrush BackgroundBrush =
        new SolidColorBrush(Color.FromRgb(20, 20, 20));

    private Bitmap? _bitmap;
    private ViewerMode _mode = ViewerMode.FitWindow;
    private double _manualZoom = 1d;
    private Vector _pan;
    private bool _dragging;
    private Point _lastPointer;

    public Bitmap? Bitmap => _bitmap;
    public ViewerMode Mode => _mode;
    public double ZoomFactor => GetZoomFactor();

    public ViewerSurface()
    {
        ClipToBounds = true;
        Focusable = true;
    }

    public void SetBitmap(Bitmap bitmap, bool resetView)
    {
        _bitmap = bitmap;

        if (resetView)
        {
            _mode = ViewerMode.FitWindow;
            _manualZoom = 1d;
            _pan = default;
        }

        InvalidateVisual();
    }

    public void SetMode(ViewerMode mode)
    {
        _mode = mode;

        if (mode is ViewerMode.FitWindow or ViewerMode.ActualSize)
        {
            _pan = default;
        }

        if (mode == ViewerMode.ActualSize)
        {
            _manualZoom = 1d;
        }

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(BackgroundBrush, Bounds);

        if (_bitmap is null || Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        var destination = CalculateDestinationRect(GetZoomFactor());
        context.DrawImage(_bitmap, destination);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (_bitmap is null || Math.Abs(e.Delta.Y) < double.Epsilon)
        {
            return;
        }

        var factor = Math.Pow(1.15, e.Delta.Y);
        ZoomAt(e.GetPosition(this), factor);
        e.Handled = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var point = e.GetCurrentPoint(this);
        if (!point.Properties.IsLeftButtonPressed || _bitmap is null)
        {
            return;
        }

        _dragging = true;
        _lastPointer = point.Position;
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_dragging || _bitmap is null)
        {
            return;
        }

        var point = e.GetPosition(this);
        var delta = point - _lastPointer;
        _lastPointer = point;

        _pan += delta;
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!_dragging)
        {
            return;
        }

        _dragging = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        InvalidateVisual();
    }

    private void ZoomAt(Point anchor, double factor)
    {
        if (_bitmap is null)
        {
            return;
        }

        var oldZoom = GetZoomFactor();
        var oldRect = CalculateDestinationRect(oldZoom);

        if (oldRect.Width <= 0 || oldRect.Height <= 0)
        {
            return;
        }

        var normalizedX = (anchor.X - oldRect.X) / oldRect.Width;
        var normalizedY = (anchor.Y - oldRect.Y) / oldRect.Height;

        var newZoom = Math.Clamp(oldZoom * factor, 0.05d, 32d);
        var imageSize = GetImageDipSize();

        var newWidth = imageSize.Width * newZoom;
        var newHeight = imageSize.Height * newZoom;

        var newLeft = anchor.X - normalizedX * newWidth;
        var newTop = anchor.Y - normalizedY * newHeight;

        var newCenter = new Point(
            newLeft + newWidth / 2d,
            newTop + newHeight / 2d);

        _mode = ViewerMode.ManualZoom;
        _manualZoom = newZoom;
        _pan = newCenter - Bounds.Center;

        InvalidateVisual();
    }

    private Rect CalculateDestinationRect(double zoom)
    {
        var imageSize = GetImageDipSize();
        var width = imageSize.Width * zoom;
        var height = imageSize.Height * zoom;

        var center = Bounds.Center + _pan;

        return new Rect(
            center.X - width / 2d,
            center.Y - height / 2d,
            width,
            height);
    }

    private double GetZoomFactor()
    {
        if (_bitmap is null)
        {
            return 1d;
        }

        return _mode switch
        {
            ViewerMode.ActualSize => 1d,
            ViewerMode.ManualZoom => _manualZoom,
            _ => CalculateFitZoom()
        };
    }

    private double CalculateFitZoom()
    {
        var imageSize = GetImageDipSize();

        if (imageSize.Width <= 0 ||
            imageSize.Height <= 0 ||
            Bounds.Width <= 0 ||
            Bounds.Height <= 0)
        {
            return 1d;
        }

        return Math.Min(
            Bounds.Width / imageSize.Width,
            Bounds.Height / imageSize.Height);
    }

    private Size GetImageDipSize()
    {
        if (_bitmap is null)
        {
            return default;
        }

        var renderScaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d;
        renderScaling = Math.Max(renderScaling, 0.1d);

        return new Size(
            _bitmap.PixelSize.Width / renderScaling,
            _bitmap.PixelSize.Height / renderScaling);
    }
}
