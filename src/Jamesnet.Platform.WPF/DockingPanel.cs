using Jamesnet.Foundation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Jamesnet.Platform.Wpf
{
    public class DockingPanel : ContentControl, IDockable
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(DockingPanel),
                new PropertyMetadata("Panel"));

        public static readonly DependencyProperty DockPositionProperty =
            DependencyProperty.Register(nameof(DockPosition), typeof(DockPosition), typeof(DockingPanel),
                new PropertyMetadata(DockPosition.None));

        public static readonly DependencyProperty IsFloatingProperty =
            DependencyProperty.Register(nameof(IsFloating), typeof(bool), typeof(DockingPanel),
                new PropertyMetadata(false));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public DockPosition DockPosition
        {
            get => (DockPosition)GetValue(DockPositionProperty);
            set => SetValue(DockPositionProperty, value);
        }

        public bool IsFloating
        {
            get => (bool)GetValue(IsFloatingProperty);
            set => SetValue(IsFloatingProperty, value);
        }

        private bool _isDragging;
        private Point _dragStartPoint;
        private Point _dragStartScreenPoint;
        private bool _hasDetached;
        private DockingManager _manager;

        private const double DetachThreshold = 20.0;

        static DockingPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockingPanel),
                new FrameworkPropertyMetadata(typeof(DockingPanel)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Header") is Border headerBorder)
            {
                headerBorder.MouseLeftButtonDown += OnHeaderMouseLeftButtonDown;
                headerBorder.MouseMove += OnHeaderMouseMove;
                headerBorder.MouseLeftButtonUp += OnHeaderMouseLeftButtonUp;
            }
        }

        private DockingManager FindDockingManager()
        {
            if (_manager != null) return _manager;

            DependencyObject current = this;
            while (current != null)
            {
                if (current is DockingManager manager)
                {
                    _manager = manager;
                    return manager;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void OnHeaderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsFloating) return;

            var manager = FindDockingManager();
            if (manager == null) return;

            _isDragging = true;
            _hasDetached = false;
            _dragStartPoint = e.GetPosition(manager);
            _dragStartScreenPoint = PointToScreen(e.GetPosition(this));

            ((UIElement)sender).CaptureMouse();
            manager.BeginDrag(this, _dragStartPoint);
        }

        // DockingPanel.cs

        private void OnHeaderMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || IsFloating) return;

            var manager = FindDockingManager();
            if (manager == null) return;

            var currentScreenPoint = PointToScreen(e.GetPosition(this));
            var delta = currentScreenPoint - _dragStartScreenPoint;
            var distance = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

            if (!_hasDetached && distance > DetachThreshold)
            {
                _hasDetached = true;
                ((UIElement)sender).ReleaseMouseCapture();
                _isDragging = false;

                var relativePoint = e.GetPosition(this);

                manager.DetachPanel(this, currentScreenPoint, relativePoint);
                return;
            }

            if (!_hasDetached)
            {
                manager.UpdateDrag(e.GetPosition(manager));
            }
        }

        private void OnHeaderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging || IsFloating) return;

            _isDragging = false;
            ((UIElement)sender).ReleaseMouseCapture();

            var manager = FindDockingManager();
            if (manager != null && !_hasDetached)
            {
                manager.EndDrag(e.GetPosition(manager));
            }
        }

        public void ResetManagerCache()
        {
            _manager = null;
        }
    }

}
