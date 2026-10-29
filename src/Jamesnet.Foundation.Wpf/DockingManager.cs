using Jamesnet.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace Jamesnet.Platform.Wpf;

public class DockingManager : ContentControl
{
    private Grid _rootGrid;
    private Canvas _overlayCanvas;
    private Rectangle _dockPreview;
    private DockingCompass _compass;
    private DockingPanel _targetDockPanel;
    private DockingPanel _draggingPanel;
    private Point _tabDragStartPoint;
    private bool _isTabDragging;

    private readonly Dictionary<DockPosition, FrameworkElement> _dockedPanels = new();
    private readonly List<FloatingWindow> _floatingWindows = new();

    public static readonly DependencyProperty LeftPanelWidthProperty =
        DependencyProperty.Register(nameof(LeftPanelWidth), typeof(double), typeof(DockingManager), new PropertyMetadata(200.0));
    public static readonly DependencyProperty RightPanelWidthProperty =
        DependencyProperty.Register(nameof(RightPanelWidth), typeof(double), typeof(DockingManager), new PropertyMetadata(200.0));
    public static readonly DependencyProperty TopPanelHeightProperty =
        DependencyProperty.Register(nameof(TopPanelHeight), typeof(double), typeof(DockingManager), new PropertyMetadata(150.0));
    public static readonly DependencyProperty BottomPanelHeightProperty =
        DependencyProperty.Register(nameof(BottomPanelHeight), typeof(double), typeof(DockingManager), new PropertyMetadata(150.0));

    public double LeftPanelWidth { get => (double)GetValue(LeftPanelWidthProperty); set => SetValue(LeftPanelWidthProperty, value); }
    public double RightPanelWidth { get => (double)GetValue(RightPanelWidthProperty); set => SetValue(RightPanelWidthProperty, value); }
    public double TopPanelHeight { get => (double)GetValue(TopPanelHeightProperty); set => SetValue(TopPanelHeightProperty, value); }
    public double BottomPanelHeight { get => (double)GetValue(BottomPanelHeightProperty); set => SetValue(BottomPanelHeightProperty, value); }

    static DockingManager()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DockingManager), new FrameworkPropertyMetadata(typeof(DockingManager)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _rootGrid = GetTemplateChild("PART_RootGrid") as Grid;
        _overlayCanvas = GetTemplateChild("PART_OverlayCanvas") as Canvas;
        InitializeGrid();
        InitializePreview();
        InitializeCompass();
    }

    private void InitializeGrid()
    {
        if (_rootGrid == null || _rootGrid.ColumnDefinitions.Count > 0) return; // 템플릿이 다시 적용돼도 한 번만 구성
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0) });
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0) });
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0) });
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0) });

        var leftSplitter = CreateSplitter(1, 0, 3, true);
        var rightSplitter = CreateSplitter(3, 0, 3, true);
        _rootGrid.Children.Add(leftSplitter);
        _rootGrid.Children.Add(rightSplitter);
    }

    private GridSplitter CreateSplitter(int col, int row, int rowSpan, bool isVertical)
    {
        var splitter = new GridSplitter { Width = isVertical ? 4 : double.NaN, Height = isVertical ? double.NaN : 4, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Background = Brushes.Transparent, ResizeBehavior = GridResizeBehavior.PreviousAndNext };
        Grid.SetColumn(splitter, col); Grid.SetRow(splitter, row); Grid.SetRowSpan(splitter, rowSpan);
        return splitter;
    }

    private void InitializePreview()
    {
        _dockPreview = new Rectangle { Fill = new SolidColorBrush(Color.FromArgb(80, 0, 122, 204)), Stroke = new SolidColorBrush(Color.FromRgb(0, 122, 204)), StrokeThickness = 2, Visibility = Visibility.Collapsed, IsHitTestVisible = false };
        _overlayCanvas?.Children.Add(_dockPreview);
    }

    private void InitializeCompass()
    {
        _compass = new DockingCompass { Visibility = Visibility.Collapsed, IsHitTestVisible = true };
        _overlayCanvas?.Children.Add(_compass);
    }

    public void AddPanel(FrameworkElement content, DockPosition position)
    {
        if (_rootGrid == null) return;
        if (content is DockingPanel panel) { panel.DockPosition = position; panel.IsFloating = false; panel.ResetManagerCache(); }
        if (_dockedPanels.TryGetValue(position, out var existingControl) && position == DockPosition.Center && content is DockingPanel p) { DockAsTab(existingControl, p); return; }
        _dockedPanels[position] = content;
        SetPanelGridPosition(content, position);
        _rootGrid.Children.Add(content);
        UpdateGridSizes();
    }

    private void SetPanelGridPosition(FrameworkElement element, DockPosition position)
    {
        Grid.SetColumnSpan(element, 1); Grid.SetRowSpan(element, 1);
        switch (position)
        {
            case DockPosition.Left: Grid.SetColumn(element, 0); Grid.SetRow(element, 0); Grid.SetRowSpan(element, 3); break;
            case DockPosition.Right: Grid.SetColumn(element, 4); Grid.SetRow(element, 0); Grid.SetRowSpan(element, 3); break;
            case DockPosition.Top: Grid.SetColumn(element, 2); Grid.SetRow(element, 0); break;
            case DockPosition.Bottom: Grid.SetColumn(element, 2); Grid.SetRow(element, 2); break;
            case DockPosition.Center: Grid.SetColumn(element, 2); Grid.SetRow(element, 1); break;
        }
    }

    private void UpdateGridSizes()
    {
        if (_rootGrid == null) return;
        _rootGrid.ColumnDefinitions[0].Width = _dockedPanels.ContainsKey(DockPosition.Left) ? new GridLength(LeftPanelWidth) : new GridLength(0);
        _rootGrid.ColumnDefinitions[1].Width = _dockedPanels.ContainsKey(DockPosition.Left) ? new GridLength(4) : new GridLength(0);
        _rootGrid.ColumnDefinitions[4].Width = _dockedPanels.ContainsKey(DockPosition.Right) ? new GridLength(RightPanelWidth) : new GridLength(0);
        _rootGrid.ColumnDefinitions[3].Width = _dockedPanels.ContainsKey(DockPosition.Right) ? new GridLength(4) : new GridLength(0);
        _rootGrid.RowDefinitions[0].Height = _dockedPanels.ContainsKey(DockPosition.Top) ? new GridLength(TopPanelHeight) : new GridLength(0);
        _rootGrid.RowDefinitions[2].Height = _dockedPanels.ContainsKey(DockPosition.Bottom) ? new GridLength(BottomPanelHeight) : new GridLength(0);
    }

    public void BeginDrag(DockingPanel panel, Point position) { _draggingPanel = panel; ShowDockPreview(true); UpdateDrag(position); }
    public void UpdateDrag(Point position) { if (_draggingPanel == null || _overlayCanvas == null) return; UpdateDockPreviewFromScreen(PointToScreen(position)); }
    public void EndDrag(Point position) { if (_draggingPanel == null) return; ShowDockPreview(false); _draggingPanel = null; if (_compass != null) _compass.Visibility = Visibility.Collapsed; }

    public void DetachPanel(DockingPanel panel, Point screenPoint, Point dragOffset)
    {
        var oldPosition = panel.DockPosition;
        var parent = panel.Parent;

        if (parent is TabItem tabItem && tabItem.Parent is TabControl tabControl)
        {
            tabItem.Content = null; // 패널을 TabItem에서 먼저 떼어야 FloatingWindow에 넣을 수 있다
            tabControl.Items.Remove(tabItem);
            if (tabControl.Items.Count == 0) { if (tabControl.Parent is Grid pg) { pg.Children.Remove(tabControl); RemoveFromDictionary(tabControl); PruneTree(pg); } }
            else if (tabControl.Items.Count == 1)
            {
                var lastTab = tabControl.Items[0] as TabItem;
                var content = lastTab.Content as DockingPanel;
                if (content != null)
                {
                    lastTab.Content = null; tabControl.Items.Clear();
                    if (tabControl.Parent is Grid pg) { int r = Grid.GetRow(tabControl), c = Grid.GetColumn(tabControl), rs = Grid.GetRowSpan(tabControl), cs = Grid.GetColumnSpan(tabControl); pg.Children.Remove(tabControl); pg.Children.Add(content); Grid.SetRow(content, r); Grid.SetColumn(content, c); Grid.SetRowSpan(content, rs); Grid.SetColumnSpan(content, cs); UpdateDockDictionary(tabControl, content); }
                }
            }
        }
        else if (parent is Panel p) { p.Children.Remove(panel); if (p is Grid pg) PruneTree(pg); }

        if (_dockedPanels.ContainsKey(oldPosition) && _dockedPanels[oldPosition] == panel) _dockedPanels.Remove(oldPosition);
        panel.DockPosition = DockPosition.None; panel.IsFloating = true;

        var floatingWindow = new FloatingWindow(panel) { SourceManager = this, Owner = Window.GetWindow(this) };
        var source = PresentationSource.FromVisual(this);
        double sx = source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0, sy = source?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;

        floatingWindow.Left = (screenPoint.X / sx) - dragOffset.X; floatingWindow.Top = (screenPoint.Y / sy) - dragOffset.Y;
        _floatingWindows.Add(floatingWindow); floatingWindow.Closed += (s, e) => _floatingWindows.Remove(floatingWindow);
        floatingWindow.Show(); floatingWindow.StartDragging(screenPoint, dragOffset);
        UpdateGridSizes(); ShowDockPreview(false); _draggingPanel = null;
    }

    public void ShowDockPreview(bool show) { if (_dockPreview != null) _dockPreview.Visibility = show ? Visibility.Visible : Visibility.Collapsed; if (!show && _compass != null) _compass.Visibility = Visibility.Collapsed; }

    private DockingPanel GetDockingPanelAt(Point pt)
    {
        DockingPanel found = null;
        VisualTreeHelper.HitTest(_rootGrid, (obj) => (obj == _overlayCanvas || obj == _compass || obj == _dockPreview || (obj is FrameworkElement fe && fe.Parent == _overlayCanvas)) ? HitTestFilterBehavior.ContinueSkipSelfAndChildren : HitTestFilterBehavior.Continue, (result) => { var p = FindParent<DockingPanel>(result.VisualHit); if (p != null) { found = p; return HitTestResultBehavior.Stop; } return HitTestResultBehavior.Continue; }, new PointHitTestParameters(pt));
        return found;
    }

    public void UpdateDockPreviewFromScreen(Point screenPoint)
    {
        var local = PointFromScreen(screenPoint);
        if (!IsPointInBounds(local)) { ShowDockPreview(false); return; }
        if (_compass.Visibility == Visibility.Visible)
        {
            var mc = TranslatePoint(local, _compass);
            if (mc.X >= -10 && mc.X <= _compass.ActualWidth + 10 && mc.Y >= -10 && mc.Y <= _compass.ActualHeight + 10) { if (_compass.UpdateHover(mc) && _targetDockPanel != null) UpdatePreviewForCompass(_targetDockPanel, _compass.CurrentHover); else if (_targetDockPanel != null) SetPreviewRectForPanel(_targetDockPanel); return; }
        }
        var target = GetDockingPanelAt(local);
        if (target != null && target != _draggingPanel && !target.IsFloating) { _targetDockPanel = target; var pc = target.TranslatePoint(new Point(target.ActualWidth / 2, target.ActualHeight / 2), _overlayCanvas); Canvas.SetLeft(_compass, pc.X - (_compass.Width / 2)); Canvas.SetTop(_compass, pc.Y - (_compass.Height / 2)); _compass.Visibility = Visibility.Visible; _compass.UpdateHover(new Point(-999, -999)); SetPreviewRectForPanel(target); }
        else { _compass.Visibility = Visibility.Collapsed; _targetDockPanel = null; var ep = GetEdgeDockPosition(local); if (ep != DockPosition.None) UpdatePreviewRect(ep); else _dockPreview.Visibility = Visibility.Collapsed; }
    }

    private void SetPreviewRectForPanel(DockingPanel target) { var tp = target.TranslatePoint(new Point(0, 0), _overlayCanvas); SetPreviewRect(tp.X, tp.Y, target.ActualWidth, target.ActualHeight); }
    private void UpdatePreviewForCompass(DockingPanel target, CompassTarget ct)
    {
        var tp = target.TranslatePoint(new Point(0, 0), _overlayCanvas);
        double w = target.ActualWidth, h = target.ActualHeight, px = tp.X, py = tp.Y, pw = w, ph = h;
        switch (ct) { case CompassTarget.Left: pw = w / 2; break; case CompassTarget.Right: px = tp.X + w / 2; pw = w / 2; break; case CompassTarget.Top: ph = h / 2; break; case CompassTarget.Bottom: py = tp.Y + h / 2; ph = h / 2; break; }
        SetPreviewRect(px, py, pw, ph);
    }

    public bool TryDockFromScreen(FloatingWindow fw, Point sp)
    {
        var lp = PointFromScreen(sp);
        if (!IsPointInBounds(lp)) { ShowDockPreview(false); return false; }
        fw.ForceUnsubscribe(); var panel = fw.Panel; fw.Content = null; panel.IsFloating = false; panel.ResetManagerCache();
        bool docked = false;
        if (_compass.Visibility == Visibility.Visible && _compass.CurrentHover != CompassTarget.None && _targetDockPanel != null) docked = _compass.CurrentHover == CompassTarget.Center ? DockAsTab(_targetDockPanel, panel) : SplitAndDock(_targetDockPanel, panel, CompassTargetToDockPos(_compass.CurrentHover));
        else
        {
            var pos = GetEdgeDockPosition(lp);
            if (pos != DockPosition.None) { if (!_dockedPanels.TryGetValue(pos, out var existing)) { AddPanel(panel, pos); docked = true; } else docked = SplitAndDock(existing, panel, pos); }
        }
        ShowDockPreview(false);
        // 도킹에 실패하면 패널을 떠 있는 창으로 되돌린다. (그대로 두면 창이 닫히며 패널이 사라진다)
        if (!docked) { fw.Content = panel; panel.IsFloating = true; }
        return docked;
    }

    private DockPosition CompassTargetToDockPos(CompassTarget t) => t switch { CompassTarget.Left => DockPosition.Left, CompassTarget.Right => DockPosition.Right, CompassTarget.Top => DockPosition.Top, CompassTarget.Bottom => DockPosition.Bottom, _ => DockPosition.Center };
    private DockPosition GetEdgeDockPosition(Point p) { double edge = _compass.Visibility == Visibility.Visible ? 30 : 80; if (p.X < edge) return DockPosition.Left; if (p.X > ActualWidth - edge) return DockPosition.Right; if (p.Y < edge) return DockPosition.Top; if (p.Y > ActualHeight - edge) return DockPosition.Bottom; return DockPosition.Center; }

    private bool DockAsTab(FrameworkElement existing, DockingPanel newPanel)
    {
        TabControl tc = existing as TabControl ?? FindParent<TabControl>(existing);
        if (tc != null) { var nt = CreateTabItem(newPanel); tc.Items.Add(nt); tc.SelectedItem = nt; tc.TabStripPlacement = newPanel.TabPlacement; SyncTabGroupOptions(tc, newPanel.IsHeaderVisible); return true; }
        var parent = existing.Parent as Panel; if (parent == null) return false;
        var ntc = new TabControl { TabStripPlacement = newPanel.TabPlacement };
        var t1 = CreateTabItem(existing is DockingPanel dp ? dp : new DockingPanel { Header = "Panel", Content = existing });
        var t2 = CreateTabItem(newPanel);
        ntc.Items.Add(t1); ntc.Items.Add(t2); ntc.SelectedIndex = 1; SyncTabGroupOptions(ntc, newPanel.IsHeaderVisible);
        int r = Grid.GetRow(existing), c = Grid.GetColumn(existing), rs = Grid.GetRowSpan(existing), cs = Grid.GetColumnSpan(existing);
        parent.Children.Remove(existing); parent.Children.Add(ntc); Grid.SetRow(ntc, r); Grid.SetColumn(ntc, c); Grid.SetRowSpan(ntc, rs); Grid.SetColumnSpan(ntc, cs); UpdateDockDictionary(existing, ntc);
        return true;
    }

    private TabItem CreateTabItem(DockingPanel panel)
    {
        var tabItem = new TabItem { Header = panel.Header, Content = panel };
        tabItem.PreviewMouseLeftButtonDown += OnTabPreviewMouseDown;
        tabItem.PreviewMouseMove += OnTabPreviewMouseMove;
        tabItem.PreviewMouseLeftButtonUp += OnTabPreviewMouseUp;
        return tabItem;
    }

    private void OnTabPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TabItem ti) { _tabDragStartPoint = e.GetPosition(this); _isTabDragging = true; ti.CaptureMouse(); }
    }

    private void OnTabPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isTabDragging || !(sender is TabItem ti)) return;
        var delta = e.GetPosition(this) - _tabDragStartPoint;
        if (Math.Abs(delta.X) > 10 || Math.Abs(delta.Y) > 10)
        {
            _isTabDragging = false; ti.ReleaseMouseCapture();
            if (ti.Content is DockingPanel panel) DetachPanel(panel, PointToScreen(e.GetPosition(this)), e.GetPosition(ti));
        }
    }

    private void OnTabPreviewMouseUp(object sender, MouseButtonEventArgs e) { _isTabDragging = false; if (sender is TabItem ti) ti.ReleaseMouseCapture(); }

    private void SyncTabGroupOptions(TabControl tc, bool hv) { foreach (TabItem item in tc.Items) if (item.Content is DockingPanel p) p.IsHeaderVisible = hv; }

    private bool SplitAndDock(FrameworkElement existing, DockingPanel newPanel, DockPosition pos)
    {
        FrameworkElement target = FindParent<TabControl>(existing) ?? existing;
        var parent = target.Parent as Panel; if (parent == null) return false;
        var sg = new Grid(); bool useCol = (pos == DockPosition.Left || pos == DockPosition.Right);
        if (useCol) { sg.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); sg.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) }); sg.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); }
        else { sg.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); sg.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4) }); sg.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); }
        int r = Grid.GetRow(target), c = Grid.GetColumn(target), rs = Grid.GetRowSpan(target), cs = Grid.GetColumnSpan(target);
        parent.Children.Remove(target); parent.Children.Add(sg); Grid.SetRow(sg, r); Grid.SetColumn(sg, c); Grid.SetRowSpan(sg, rs); Grid.SetColumnSpan(sg, cs);
        bool isFirst = (pos == DockPosition.Left || pos == DockPosition.Top);
        var f = isFirst ? (UIElement)newPanel : target; var s = isFirst ? (UIElement)target : newPanel;
        sg.Children.Add(f); sg.Children.Add(s);
        if (useCol) { SetSubGridPos(f, 0, 0); SetSubGridPos(s, 0, 2); } else { SetSubGridPos(f, 0, 0); SetSubGridPos(s, 2, 0); }
        newPanel.DockPosition = pos;
        var gs = new GridSplitter { HorizontalAlignment = useCol ? HorizontalAlignment.Center : HorizontalAlignment.Stretch, VerticalAlignment = useCol ? VerticalAlignment.Stretch : VerticalAlignment.Center, Background = Brushes.Gray, ShowsPreview = true };
        if (useCol) { gs.Width = 4; Grid.SetRow(gs, 0); Grid.SetColumn(gs, 1); } else { gs.Height = 4; Grid.SetRow(gs, 1); Grid.SetColumn(gs, 0); }
        sg.Children.Add(gs); if (_dockedPanels.ContainsKey(pos) && _dockedPanels[pos] == target) _dockedPanels[pos] = sg;
        return true;
    }

    private void PruneTree(Grid g)
    {
        if (g == null || g == _rootGrid) return;
        var cc = g.Children.OfType<FrameworkElement>().Where(c => !(c is GridSplitter)).ToList();
        var p = g.Parent as Grid;
        if (cc.Count == 0) { if (p != null) { p.Children.Remove(g); RemoveFromDictionary(g); PruneTree(p); } }
        else if (cc.Count == 1)
        {
            var s = cc[0]; g.Children.Remove(s);
            if (p != null) { int r = Grid.GetRow(g), c = Grid.GetColumn(g), rs = Grid.GetRowSpan(g), cs = Grid.GetColumnSpan(g); p.Children.Remove(g); p.Children.Add(s); Grid.SetRow(s, r); Grid.SetColumn(s, c); Grid.SetRowSpan(s, rs); Grid.SetColumnSpan(s, cs); UpdateDockDictionary(g, s); PruneTree(p); }
            else if (g.Parent == _rootGrid) { _rootGrid.Children.Remove(g); _rootGrid.Children.Add(s); Grid.SetRow(s, Grid.GetRow(g)); Grid.SetColumn(s, Grid.GetColumn(g)); Grid.SetRowSpan(s, Grid.GetRowSpan(g)); Grid.SetColumnSpan(s, Grid.GetColumnSpan(g)); UpdateDockDictionary(g, s); }
        }
    }

    private void RemoveFromDictionary(FrameworkElement e) { var keys = _dockedPanels.Where(k => k.Value == e).Select(k => k.Key).ToList(); foreach (var k in keys) _dockedPanels.Remove(k); }
    private void UpdateDockDictionary(FrameworkElement o, FrameworkElement n) { var keys = _dockedPanels.Where(k => k.Value == o).Select(k => k.Key).ToList(); foreach (var k in keys) _dockedPanels[k] = n; }
    private void SetSubGridPos(UIElement e, int r, int c) { Grid.SetRow(e, r); Grid.SetColumn(e, c); Grid.SetRowSpan(e, 1); Grid.SetColumnSpan(e, 1); }
    private bool IsPointInBounds(Point p) => p.X >= 0 && p.X <= ActualWidth && p.Y >= 0 && p.Y <= ActualHeight;
    private void UpdatePreviewRect(DockPosition pos)
    {
        double w = ActualWidth, h = ActualHeight, l = _dockedPanels.ContainsKey(DockPosition.Left) ? LeftPanelWidth : 0, r = _dockedPanels.ContainsKey(DockPosition.Right) ? RightPanelWidth : 0, t = _dockedPanels.ContainsKey(DockPosition.Top) ? TopPanelHeight : 0, b = _dockedPanels.ContainsKey(DockPosition.Bottom) ? BottomPanelHeight : 0;
        switch (pos) { case DockPosition.Left: SetPreviewRect(0, 0, LeftPanelWidth, h); break; case DockPosition.Right: SetPreviewRect(w - RightPanelWidth, 0, RightPanelWidth, h); break; case DockPosition.Top: SetPreviewRect(l, 0, w - l - r, TopPanelHeight); break; case DockPosition.Bottom: SetPreviewRect(l, h - BottomPanelHeight, w - l - r, BottomPanelHeight); break; case DockPosition.Center: SetPreviewRect(l, t, w - l - r, h - t - b); break; default: _dockPreview.Visibility = Visibility.Collapsed; break; }
    }
    private void SetPreviewRect(double x, double y, double w, double h) { Canvas.SetLeft(_dockPreview, x); Canvas.SetTop(_dockPreview, y); _dockPreview.Width = Math.Max(0, w); _dockPreview.Height = Math.Max(0, h); _dockPreview.Visibility = Visibility.Visible; }
    private T FindParent<T>(DependencyObject c) where T : DependencyObject { if (c == null) return null; var p = VisualTreeHelper.GetParent(c); return p is T tp ? tp : FindParent<T>(p); }
}