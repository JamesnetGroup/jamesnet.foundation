using Jamesnet.Foundation;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Jamesnet.Platform.Wpf;

public class FloatingWindow : Window
{
    public DockingPanel Panel { get; private set; }
    public DockingManager SourceManager { get; set; }
    private bool _isDragging;
    private Point _dragOffset;

    public FloatingWindow(DockingPanel panel)
    {
        Panel = panel; WindowStyle = WindowStyle.None; AllowsTransparency = true; Background = Brushes.Transparent; ResizeMode = ResizeMode.CanResizeWithGrip; Width = 300; Height = 200; Content = panel;
    }

    private Point GetLogicalPoint(Point sp) { var s = PresentationSource.FromVisual(this); return s?.CompositionTarget != null ? s.CompositionTarget.TransformFromDevice.Transform(sp) : sp; }

    public void StartDragging(Point sp, Point offset)
    {
        _isDragging = true; _dragOffset = offset;
        var lp = GetLogicalPoint(sp); Left = lp.X - _dragOffset.X; Top = lp.Y - _dragOffset.Y;
        SourceManager?.ShowDockPreview(true); CaptureMouse();
    }

    public void ForceUnsubscribe() { ReleaseMouseCapture(); }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!_isDragging) return;
        var cp = PointToScreen(e.GetPosition(this)); var lp = GetLogicalPoint(cp);
        Left = lp.X - _dragOffset.X; Top = lp.Y - _dragOffset.Y;
        SourceManager?.UpdateDockPreviewFromScreen(cp);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false; ReleaseMouseCapture();
        if (SourceManager?.TryDockFromScreen(this, PointToScreen(e.GetPosition(this))) != true) SourceManager?.ShowDockPreview(false); else Close();
    }
}