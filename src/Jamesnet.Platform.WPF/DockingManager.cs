using Jamesnet.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Jamesnet.Platform.Wpf;

public class DockingManager : ContentControl
{
    private Grid _rootGrid;
    private Canvas _overlayCanvas;
    private Rectangle _dockPreview;
    private DockingCompass _compass;
    private DockingPanel _targetDockPanel;
    private DockingPanel _draggingPanel;
    private DockPosition _previewPosition;

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

    public double LeftPanelWidth
    {
        get => (double)GetValue(LeftPanelWidthProperty);
        set => SetValue(LeftPanelWidthProperty, value);
    }
    public double RightPanelWidth
    {
        get => (double)GetValue(RightPanelWidthProperty);
        set => SetValue(RightPanelWidthProperty, value);
    }
    public double TopPanelHeight
    {
        get => (double)GetValue(TopPanelHeightProperty);
        set => SetValue(TopPanelHeightProperty, value);
    }
    public double BottomPanelHeight
    {
        get => (double)GetValue(BottomPanelHeightProperty);
        set => SetValue(BottomPanelHeightProperty, value);
    }

    static DockingManager()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DockingManager),
            new FrameworkPropertyMetadata(typeof(DockingManager)));
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
        if (_rootGrid == null) return;

        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0) });
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0) });

        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0) });
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0) });
    }

    private void InitializePreview()
    {
        _dockPreview = new Rectangle
        {
            Fill = new SolidColorBrush(Color.FromArgb(80, 0, 122, 204)),
            Stroke = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
            StrokeThickness = 2,
            Visibility = Visibility.Collapsed,
            IsHitTestVisible = false
        };
        _overlayCanvas?.Children.Add(_dockPreview);
    }

    private void InitializeCompass()
    {
        _compass = new DockingCompass
        {
            Visibility = Visibility.Collapsed,
            IsHitTestVisible = true
        };
        _overlayCanvas?.Children.Add(_compass);
    }

    public void AddPanel(FrameworkElement content, DockPosition position)
    {
        if (_rootGrid == null) return;

        if (content is DockingPanel panel)
        {
            panel.DockPosition = position;
            panel.IsFloating = false;
            panel.ResetManagerCache();
        }

        _dockedPanels[position] = content;
        SetPanelGridPosition(content, position);
        _rootGrid.Children.Add(content);
        UpdateGridSizes();
    }

    private void SetPanelGridPosition(FrameworkElement element, DockPosition position)
    {
        Grid.SetColumnSpan(element, 1);
        Grid.SetRowSpan(element, 1);

        switch (position)
        {
            case DockPosition.Left:
                Grid.SetColumn(element, 0); Grid.SetRow(element, 0); Grid.SetRowSpan(element, 3);
                break;
            case DockPosition.Right:
                Grid.SetColumn(element, 4); Grid.SetRow(element, 0); Grid.SetRowSpan(element, 3);
                break;
            case DockPosition.Top:
                Grid.SetColumn(element, 2); Grid.SetRow(element, 0);
                break;
            case DockPosition.Bottom:
                Grid.SetColumn(element, 2); Grid.SetRow(element, 2);
                break;
            case DockPosition.Center:
                Grid.SetColumn(element, 2); Grid.SetRow(element, 1);
                break;
        }
    }

    private void UpdateGridSizes()
    {
        if (_rootGrid == null) return;

        bool hasLeft = _dockedPanels.ContainsKey(DockPosition.Left);
        bool hasRight = _dockedPanels.ContainsKey(DockPosition.Right);
        bool hasTop = _dockedPanels.ContainsKey(DockPosition.Top);
        bool hasBottom = _dockedPanels.ContainsKey(DockPosition.Bottom);

        _rootGrid.ColumnDefinitions[0].Width = hasLeft ? new GridLength(LeftPanelWidth) : new GridLength(0);
        _rootGrid.ColumnDefinitions[4].Width = hasRight ? new GridLength(RightPanelWidth) : new GridLength(0);
        _rootGrid.RowDefinitions[0].Height = hasTop ? new GridLength(TopPanelHeight) : new GridLength(0);
        _rootGrid.RowDefinitions[2].Height = hasBottom ? new GridLength(BottomPanelHeight) : new GridLength(0);
    }

    public void BeginDrag(DockingPanel panel, Point position)
    {
        _draggingPanel = panel;
        ShowDockPreview(true);
        UpdateDrag(position);
    }

    public void UpdateDrag(Point position)
    {
        if (_draggingPanel == null || _overlayCanvas == null) return;
        UpdateDockPreviewFromScreen(PointToScreen(position));
    }

    public void EndDrag(Point position)
    {
        if (_draggingPanel == null) return;
        ShowDockPreview(false);
        _draggingPanel = null;
        if (_compass != null)
            _compass.Visibility = Visibility.Collapsed;
    }

    public void DetachPanel(DockingPanel panel, Point screenPoint, Point dragOffset)
    {
        var oldPosition = panel.DockPosition;
        var parent = panel.Parent;

        // 1. 탭 컨트롤에서 분리되는 경우
        if (parent is TabItem tabItem && tabItem.Parent is TabControl tabControl)
        {
            tabControl.Items.Remove(tabItem);

            // 탭이 비었으면 제거
            if (tabControl.Items.Count == 0)
            {
                if (tabControl.Parent is Grid parentGrid)
                {
                    parentGrid.Children.Remove(tabControl);
                    RemoveFromDictionary(tabControl);
                    PruneTree(parentGrid);
                }
            }
            // 하나 남았으면 패널로 복구
            else if (tabControl.Items.Count == 1)
            {
                var lastTab = tabControl.Items[0] as TabItem;
                var content = lastTab.Content as DockingPanel;

                if (content != null)
                {
                    lastTab.Content = null;
                    tabControl.Items.Clear();

                    if (tabControl.Parent is Grid parentGrid)
                    {
                        int r = Grid.GetRow(tabControl);
                        int c = Grid.GetColumn(tabControl);
                        int rs = Grid.GetRowSpan(tabControl);
                        int cs = Grid.GetColumnSpan(tabControl);

                        parentGrid.Children.Remove(tabControl);
                        parentGrid.Children.Add(content);

                        Grid.SetRow(content, r);
                        Grid.SetColumn(content, c);
                        Grid.SetRowSpan(content, rs);
                        Grid.SetColumnSpan(content, cs);

                        UpdateDockDictionary(tabControl, content);
                    }
                }
            }
        }
        // 2. 일반 Grid나 Panel에서 분리되는 경우
        else if (parent is Grid parentGrid)
        {
            parentGrid.Children.Remove(panel);
            PruneTree(parentGrid);
        }
        else if (parent is Panel p)
        {
            p.Children.Remove(panel);
        }

        if (_dockedPanels.ContainsKey(oldPosition) && _dockedPanels[oldPosition] == panel)
        {
            _dockedPanels.Remove(oldPosition);
        }

        panel.DockPosition = DockPosition.None;
        panel.IsFloating = true;

        var floatingWindow = new FloatingWindow(panel)
        {
            SourceManager = this,
            Owner = Window.GetWindow(this)
        };

        var source = PresentationSource.FromVisual(this);
        double sx = 1.0, sy = 1.0;
        if (source?.CompositionTarget != null)
        {
            sx = source.CompositionTarget.TransformToDevice.M11;
            sy = source.CompositionTarget.TransformToDevice.M22;
        }

        floatingWindow.Left = (screenPoint.X / sx) - dragOffset.X;
        floatingWindow.Top = (screenPoint.Y / sy) - dragOffset.Y;

        _floatingWindows.Add(floatingWindow);
        floatingWindow.Closed += (s, e) => _floatingWindows.Remove(floatingWindow);

        floatingWindow.Show();
        floatingWindow.StartDragging(screenPoint, dragOffset);

        UpdateGridSizes();
        ShowDockPreview(false);
        _draggingPanel = null;
    }

    public void ShowDockPreview(bool show)
    {
        if (_dockPreview != null)
            _dockPreview.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

        if (!show && _compass != null)
            _compass.Visibility = Visibility.Collapsed;
    }

    private DockingPanel GetDockingPanelAt(Point pt)
    {
        DockingPanel foundPanel = null;
        VisualTreeHelper.HitTest(_rootGrid,
            (obj) =>
            {
                if (obj == _overlayCanvas || obj == _compass || obj == _dockPreview || (obj is FrameworkElement fe && fe.Parent == _overlayCanvas))
                    return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                return HitTestFilterBehavior.Continue;
            },
            (result) =>
            {
                var panel = FindParent<DockingPanel>(result.VisualHit);
                if (panel != null)
                {
                    foundPanel = panel;
                    return HitTestResultBehavior.Stop;
                }
                return HitTestResultBehavior.Continue;
            },
            new PointHitTestParameters(pt));

        return foundPanel;
    }

    public void UpdateDockPreviewFromScreen(Point screenPoint)
    {
        var localPoint = PointFromScreen(screenPoint);

        if (!IsPointInBounds(localPoint))
        {
            ShowDockPreview(false);
            return;
        }

        // 1. 나침반 상호작용
        if (_compass.Visibility == Visibility.Visible)
        {
            var mouseOnCompass = this.TranslatePoint(localPoint, _compass);
            bool isOverCompass = mouseOnCompass.X >= -10 && mouseOnCompass.X <= _compass.ActualWidth + 10 &&
                                 mouseOnCompass.Y >= -10 && mouseOnCompass.Y <= _compass.ActualHeight + 10;

            if (isOverCompass)
            {
                if (_compass.UpdateHover(mouseOnCompass))
                {
                    if (_targetDockPanel != null)
                        UpdatePreviewForCompass(_targetDockPanel, _compass.CurrentHover);
                }
                else
                {
                    if (_targetDockPanel != null)
                        SetPreviewRectForPanel(_targetDockPanel);
                }
                return;
            }
        }

        // 2. 패널 탐색
        var targetPanel = GetDockingPanelAt(localPoint);

        if (targetPanel != null && targetPanel != _draggingPanel && !targetPanel.IsFloating)
        {
            _targetDockPanel = targetPanel;

            var panelCenter = targetPanel.TranslatePoint(new Point(targetPanel.ActualWidth / 2, targetPanel.ActualHeight / 2), _overlayCanvas);
            Canvas.SetLeft(_compass, panelCenter.X - (_compass.Width / 2));
            Canvas.SetTop(_compass, panelCenter.Y - (_compass.Height / 2));

            _compass.Visibility = Visibility.Visible;
            _compass.UpdateHover(new Point(-999, -999));

            SetPreviewRectForPanel(targetPanel);
        }
        else
        {
            _compass.Visibility = Visibility.Collapsed;
            _targetDockPanel = null;

            var edgePos = GetEdgeDockPosition(localPoint);
            if (edgePos != DockPosition.None)
            {
                UpdatePreviewRect(edgePos);
            }
            else
            {
                _dockPreview.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void SetPreviewRectForPanel(DockingPanel target)
    {
        var targetPos = target.TranslatePoint(new Point(0, 0), _overlayCanvas);
        SetPreviewRect(targetPos.X, targetPos.Y, target.ActualWidth, target.ActualHeight);
    }

    private void UpdatePreviewForCompass(DockingPanel target, CompassTarget compassTarget)
    {
        var targetPos = target.TranslatePoint(new Point(0, 0), _overlayCanvas);
        double w = target.ActualWidth;
        double h = target.ActualHeight;

        double px = targetPos.X;
        double py = targetPos.Y;
        double pw = w;
        double ph = h;

        switch (compassTarget)
        {
            case CompassTarget.Left:
                pw = w / 2;
                break;
            case CompassTarget.Right:
                px = targetPos.X + w / 2;
                pw = w / 2;
                break;
            case CompassTarget.Top:
                ph = h / 2;
                break;
            case CompassTarget.Bottom:
                py = targetPos.Y + h / 2;
                ph = h / 2;
                break;
            case CompassTarget.Center:
                // 탭 모드는 전체
                break;
        }

        SetPreviewRect(px, py, pw, ph);
    }

    public bool TryDockFromScreen(FloatingWindow floatingWindow, Point screenPoint)
    {
        var localPoint = PointFromScreen(screenPoint);
        if (!IsPointInBounds(localPoint))
        {
            ShowDockPreview(false);
            return false;
        }

        floatingWindow.ForceUnsubscribe();

        var panel = floatingWindow.Panel;
        floatingWindow.Content = null;
        panel.IsFloating = false;
        panel.ResetManagerCache();

        // 1. 나침반 정밀 도킹
        if (_compass.Visibility == Visibility.Visible && _compass.CurrentHover != CompassTarget.None && _targetDockPanel != null)
        {
            if (_compass.CurrentHover == CompassTarget.Center)
            {
                DockAsTab(_targetDockPanel, panel);
            }
            else
            {
                DockPosition splitPos = CompassTargetToDockPos(_compass.CurrentHover);
                SplitAndDock(_targetDockPanel, panel, splitPos);
            }
            ShowDockPreview(false);
            return true;
        }

        // 2. 가장자리 도킹 (빈 공간은 Center/Stretch로 처리)
        var position = GetEdgeDockPosition(localPoint);
        if (position != DockPosition.None)
        {
            if (!_dockedPanels.TryGetValue(position, out var existingControl))
            {
                AddPanel(panel, position);
            }
            else
            {
                SplitAndDock(existingControl, panel, position);
            }
            ShowDockPreview(false);
            return true;
        }

        ShowDockPreview(false);
        return false;
    }

    private DockPosition CompassTargetToDockPos(CompassTarget target)
    {
        switch (target)
        {
            case CompassTarget.Left: return DockPosition.Left;
            case CompassTarget.Right: return DockPosition.Right;
            case CompassTarget.Top: return DockPosition.Top;
            case CompassTarget.Bottom: return DockPosition.Bottom;
            default: return DockPosition.Center;
        }
    }

    private DockPosition GetEdgeDockPosition(Point point)
    {
        double width = ActualWidth;
        double height = ActualHeight;
        double edgeSize = 80;

        if (_compass.Visibility == Visibility.Visible) edgeSize = 30;

        if (point.X < edgeSize) return DockPosition.Left;
        if (point.X > width - edgeSize) return DockPosition.Right;
        if (point.Y < edgeSize) return DockPosition.Top;
        if (point.Y > height - edgeSize) return DockPosition.Bottom;

        return DockPosition.Center; // 빈 공간은 Center로 처리하여 Stretch 효과
    }

    private void DockAsTab(FrameworkElement existingControl, DockingPanel newPanel)
    {
        // 1. VisualTree를 탐색하여 TabControl을 확실하게 찾기
        TabControl targetTabControl = existingControl as TabControl;

        if (targetTabControl == null)
        {
            DependencyObject current = existingControl;
            while (current != null)
            {
                if (current is TabControl tc)
                {
                    targetTabControl = tc;
                    break;
                }
                if (current is DockingManager) break;

                current = VisualTreeHelper.GetParent(current);
            }
        }

        // 2. 이미 탭 컨트롤이라면 새 탭 추가
        if (targetTabControl != null)
        {
            var newTab = new TabItem { Header = newPanel.Header, Content = newPanel };
            targetTabControl.Items.Add(newTab);
            targetTabControl.SelectedItem = newTab;
            return;
        }

        // 3. 탭이 없다면 새로 생성하여 감싸기
        var parent = existingControl.Parent as Panel;
        if (parent == null) return;

        // [수정] 탭 헤더를 아래쪽(Bottom)으로 배치
        var newTabControl = new TabControl
        {
            TabStripPlacement = Dock.Bottom
        };

        string oldHeader = (existingControl is DockingPanel dp) ? dp.Header : "Panel";

        var tab1 = new TabItem { Header = oldHeader, Content = existingControl };
        var tab2 = new TabItem { Header = newPanel.Header, Content = newPanel };

        newTabControl.Items.Add(tab1);
        newTabControl.Items.Add(tab2);
        newTabControl.SelectedIndex = 1;

        // 기존 컨트롤의 Grid 위치 정보 복사
        int r = Grid.GetRow(existingControl);
        int c = Grid.GetColumn(existingControl);
        int rs = Grid.GetRowSpan(existingControl);
        int cs = Grid.GetColumnSpan(existingControl);

        // 부모 컨테이너에서 교체
        parent.Children.Remove(existingControl);
        parent.Children.Add(newTabControl);

        // 새 탭 컨트롤에 위치 정보 적용
        Grid.SetRow(newTabControl, r);
        Grid.SetColumn(newTabControl, c);
        Grid.SetRowSpan(newTabControl, rs);
        Grid.SetColumnSpan(newTabControl, cs);

        // 관리 딕셔너리 업데이트
        UpdateDockDictionary(existingControl, newTabControl);
    }

    private void SplitAndDock(FrameworkElement existingControl, DockingPanel newPanel, DockPosition position)
    {
        FrameworkElement actualTarget = existingControl;
        if (FindParent<TabControl>(existingControl) is TabControl parentTab)
        {
            actualTarget = parentTab;
        }

        var parentContainer = actualTarget.Parent as Panel;
        if (parentContainer == null) return;

        var splitGrid = new Grid();
        bool useColumns = (position == DockPosition.Left || position == DockPosition.Right);

        if (useColumns)
        {
            splitGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            splitGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
            splitGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }
        else
        {
            splitGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            splitGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4) });
            splitGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

        int r = Grid.GetRow(actualTarget);
        int c = Grid.GetColumn(actualTarget);
        int rs = Grid.GetRowSpan(actualTarget);
        int cs = Grid.GetColumnSpan(actualTarget);

        parentContainer.Children.Remove(actualTarget);
        parentContainer.Children.Add(splitGrid);      

        Grid.SetRow(splitGrid, r);
        Grid.SetColumn(splitGrid, c);
        Grid.SetRowSpan(splitGrid, rs);
        Grid.SetColumnSpan(splitGrid, cs);

        bool isNewPanelFirst = (position == DockPosition.Left || position == DockPosition.Top);

        var firstItem = isNewPanelFirst ? (UIElement)newPanel : actualTarget;
        var secondItem = isNewPanelFirst ? (UIElement)actualTarget : newPanel;

        splitGrid.Children.Add(firstItem);
        splitGrid.Children.Add(secondItem);

        if (useColumns)
        {
            SetSubGridPosition(firstItem, 0, 0);
            SetSubGridPosition(secondItem, 0, 2);
        }
        else
        {
            SetSubGridPosition(firstItem, 0, 0);
            SetSubGridPosition(secondItem, 2, 0);
        }

        newPanel.DockPosition = position;

        var splitter = new GridSplitter
        {
            HorizontalAlignment = useColumns ? HorizontalAlignment.Center : HorizontalAlignment.Stretch,
            VerticalAlignment = useColumns ? VerticalAlignment.Stretch : VerticalAlignment.Center,
            Background = Brushes.Gray,
            ShowsPreview = true
        };

        if (useColumns)
        {
            splitter.Width = 4;
            Grid.SetRow(splitter, 0);
            Grid.SetColumn(splitter, 1);
        }
        else
        {
            splitter.Height = 4;
            Grid.SetRow(splitter, 1);
            Grid.SetColumn(splitter, 0);
        }
        splitGrid.Children.Add(splitter);

        if (_dockedPanels.ContainsKey(position) && _dockedPanels[position] == actualTarget)
        {
            _dockedPanels[position] = splitGrid;
        }
    }

    private void PruneTree(Grid container)
    {
        if (container == null || container == _rootGrid) return;

        var contentChildren = container.Children.OfType<FrameworkElement>()
                                .Where(c => !(c is GridSplitter))
                                .ToList();

        var parent = container.Parent as Grid;

        if (contentChildren.Count == 0)
        {
            if (parent != null)
            {
                parent.Children.Remove(container);
                RemoveFromDictionary(container);
                PruneTree(parent);
            }
        }
        else if (contentChildren.Count == 1)
        {
            var survivor = contentChildren[0];
            container.Children.Remove(survivor);

            if (parent != null)
            {
                int r = Grid.GetRow(container);
                int c = Grid.GetColumn(container);
                int rs = Grid.GetRowSpan(container);
                int cs = Grid.GetColumnSpan(container);

                parent.Children.Remove(container);
                parent.Children.Add(survivor);

                Grid.SetRow(survivor, r);
                Grid.SetColumn(survivor, c);
                Grid.SetRowSpan(survivor, rs);
                Grid.SetColumnSpan(survivor, cs);

                UpdateDockDictionary(container, survivor);
                PruneTree(parent);
            }
            else if (container.Parent == _rootGrid)
            {
                _rootGrid.Children.Remove(container);
                _rootGrid.Children.Add(survivor);

                Grid.SetRow(survivor, Grid.GetRow(container));
                Grid.SetColumn(survivor, Grid.GetColumn(container));
                Grid.SetRowSpan(survivor, Grid.GetRowSpan(container));
                Grid.SetColumnSpan(survivor, Grid.GetColumnSpan(container));

                UpdateDockDictionary(container, survivor);
            }
        }
    }

    private void RemoveFromDictionary(FrameworkElement element)
    {
        var keys = _dockedPanels.Where(k => k.Value == element).Select(k => k.Key).ToList();
        foreach (var k in keys) _dockedPanels.Remove(k);
    }

    private void UpdateDockDictionary(FrameworkElement oldItem, FrameworkElement newItem)
    {
        var keys = _dockedPanels.Where(k => k.Value == oldItem).Select(k => k.Key).ToList();
        foreach (var k in keys) _dockedPanels[k] = newItem;
    }

    private void SetSubGridPosition(UIElement element, int row, int col)
    {
        Grid.SetRow(element, row);
        Grid.SetColumn(element, col);
        Grid.SetRowSpan(element, 1);
        Grid.SetColumnSpan(element, 1);
    }

    private bool IsPointInBounds(Point point)
    {
        return point.X >= 0 && point.X <= ActualWidth &&
               point.Y >= 0 && point.Y <= ActualHeight;
    }

    private void UpdatePreviewRect(DockPosition position)
    {
        double w = ActualWidth;
        double h = ActualHeight;

        double l = _dockedPanels.ContainsKey(DockPosition.Left) ? LeftPanelWidth : 0;
        double r = _dockedPanels.ContainsKey(DockPosition.Right) ? RightPanelWidth : 0;
        double t = _dockedPanels.ContainsKey(DockPosition.Top) ? TopPanelHeight : 0;
        double b = _dockedPanels.ContainsKey(DockPosition.Bottom) ? BottomPanelHeight : 0;

        switch (position)
        {
            case DockPosition.Left:
                SetPreviewRect(0, 0, LeftPanelWidth, h);
                break;
            case DockPosition.Right:
                SetPreviewRect(w - RightPanelWidth, 0, RightPanelWidth, h);
                break;
            case DockPosition.Top:
                SetPreviewRect(l, 0, w - l - r, TopPanelHeight);
                break;
            case DockPosition.Bottom:
                SetPreviewRect(l, h - BottomPanelHeight, w - l - r, BottomPanelHeight);
                break;
            case DockPosition.Center:
                SetPreviewRect(l, t, w - l - r, h - t - b);
                break;
            default:
                _dockPreview.Visibility = Visibility.Collapsed;
                break;
        }
    }

    private void SetPreviewRect(double x, double y, double w, double h)
    {
        Canvas.SetLeft(_dockPreview, x);
        Canvas.SetTop(_dockPreview, y);
        _dockPreview.Width = Math.Max(0, w);
        _dockPreview.Height = Math.Max(0, h);
        _dockPreview.Visibility = Visibility.Visible;
    }

    private void MovePanelToPosition(DockingPanel panel, DockPosition newPosition)
    {
        var oldPosition = panel.DockPosition;

        if (_dockedPanels.TryGetValue(oldPosition, out var oldContent) && oldContent == panel)
        {
            _dockedPanels.Remove(oldPosition);
        }

        if (_dockedPanels.TryGetValue(newPosition, out var existing))
        {
            _dockedPanels.Remove(newPosition);
            _rootGrid.Children.Remove(existing);

            if (existing is DockingPanel existingPanel)
            {
                existingPanel.DockPosition = oldPosition;
                _dockedPanels[oldPosition] = existingPanel;
                SetPanelGridPosition(existingPanel, oldPosition);
                _rootGrid.Children.Add(existingPanel);
            }
        }

        panel.DockPosition = newPosition;
        _dockedPanels[newPosition] = panel;
        SetPanelGridPosition(panel, newPosition);
        UpdateGridSizes();
    }

    private T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        if (child == null) return null;
        DependencyObject parentObject = VisualTreeHelper.GetParent(child);
        if (parentObject == null) return null;
        if (parentObject is T parent) return parent;
        return FindParent<T>(parentObject);
    }
}
