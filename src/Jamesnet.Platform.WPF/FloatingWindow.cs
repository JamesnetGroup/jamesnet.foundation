using Jamesnet.Foundation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Jamesnet.Platform.Wpf;

public class FloatingWindow : Window
{
    public DockingPanel Panel { get; private set; }
    public DockingManager SourceManager { get; set; }

    private bool _isDragging;
    private Point _dragOffset;
    private Border _subscribedHeader;

    public FloatingWindow(DockingPanel panel)
    {
        Panel = panel;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ResizeMode = ResizeMode.CanResizeWithGrip;
        Width = 300;
        Height = 200;
        Content = panel;
        Loaded += OnLoaded;
    }

    private Point GetLogicalPoint(Point screenPoint)
    {
        var source = PresentationSource.FromVisual(this);
        if (source?.CompositionTarget != null)
            return source.CompositionTarget.TransformFromDevice.Transform(screenPoint);
        return screenPoint;
    }

    public void StartDragging(Point currentMouseScreenPoint, Point initialOffset)
    {
        if (Panel.Template.FindName("PART_Header", Panel) is Border header)
        {
            if (header.CaptureMouse())
            {
                _isDragging = true;
                _dragOffset = initialOffset;
                var logicalMouse = GetLogicalPoint(currentMouseScreenPoint);
                Left = logicalMouse.X - _dragOffset.X;
                Top = logicalMouse.Y - _dragOffset.Y;
                SourceManager?.ShowDockPreview(true);
            }
        }
    }

    public void ForceUnsubscribe()
    {
        if (_subscribedHeader != null)
        {
            _subscribedHeader.MouseLeftButtonDown -= OnHeaderMouseDown;
            _subscribedHeader.MouseMove -= OnHeaderMouseMove;
            _subscribedHeader.MouseLeftButtonUp -= OnHeaderMouseUp;
            _subscribedHeader = null;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Panel.Template.FindName("PART_Header", Panel) is Border header)
        {
            _subscribedHeader = header;
            _subscribedHeader.MouseLeftButtonDown += OnHeaderMouseDown;
            _subscribedHeader.MouseMove += OnHeaderMouseMove;
            _subscribedHeader.MouseLeftButtonUp += OnHeaderMouseUp;
        }
    }

    private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (PresentationSource.FromVisual(this) == null) return;

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _isDragging = true;
            ((UIElement)sender).CaptureMouse();

            var mouseScreen = PointToScreen(e.GetPosition(this));
            var mouseLogical = GetLogicalPoint(mouseScreen);
            _dragOffset = new Point(mouseLogical.X - Left, mouseLogical.Y - Top);
            SourceManager?.ShowDockPreview(true);
        }
    }

    private void OnHeaderMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        if (PresentationSource.FromVisual(this) == null) return;

        var currentPhysicalPos = PointToScreen(e.GetPosition(this));
        var currentLogicalPos = GetLogicalPoint(currentPhysicalPos);

        Left = currentLogicalPos.X - _dragOffset.X;
        Top = currentLogicalPos.Y - _dragOffset.Y;

        SourceManager?.UpdateDockPreviewFromScreen(currentPhysicalPos);
    }

    private void OnHeaderMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        if (PresentationSource.FromVisual(this) == null) return;

        _isDragging = false;
        ((UIElement)sender).ReleaseMouseCapture();

        var screenPos = PointToScreen(e.GetPosition(this));

        if (SourceManager?.TryDockFromScreen(this, screenPos) == true)
            Close();
        else
            SourceManager?.ShowDockPreview(false);
    }

    protected override void OnClosed(EventArgs e)
    {
        ForceUnsubscribe();
        Content = null;
        base.OnClosed(e);
    }
}
