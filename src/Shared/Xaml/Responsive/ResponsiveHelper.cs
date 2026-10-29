using System;
using System.Collections.Generic;
#if WINUI3 || UNO
using Windows.Foundation;
#endif

namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// 모든 Responsive* attached property가 공유하는 창 너비 추적기.
///
/// - 요소가 Loaded일 때만 창 크기 변경을 구독하고 Unloaded에서 해제한다(누수 없음).
///   다시 Loaded되면 자동으로 재구독한다.
/// - 한 요소에 여러 Responsive* 속성을 붙여도 구독은 하나이고, 각 속성의 적용 함수가 모두 호출된다.
/// - 창 너비 출처: WPF는 요소가 속한 Window, WinUI3/Uno는 요소의 XamlRoot,
///   OpenSilver/UWP는 Window.Current.
/// </summary>
internal static class ResponsiveHelper
{
    internal const double DefaultBreakpoint = 768.0;

    /// <summary>값이 바뀌면 대상 요소를 추적 대상으로 등록하는 attached property를 만든다.</summary>
    internal static DependencyProperty Attach(string name, Type type, Type owner, object defaultValue, Action<FrameworkElement, double> apply) =>
        DependencyProperty.RegisterAttached(name, type, owner, new PropertyMetadata(defaultValue, (d, _) =>
        {
            if (d is FrameworkElement element)
            {
                Track(element, apply);
            }
        }));

    /// <summary>창 너비가 breakpoint 이하이면 모바일 구간이다.</summary>
    internal static bool IsMobile(double width, double breakpoint) => width <= breakpoint;

    /// <summary>value가 null 또는 NaN이면 로컬 값을 지워 원래 값으로 되돌리고, 아니면 설정한다.</summary>
    internal static void SetOrClear(DependencyObject target, DependencyProperty property, object value)
    {
        if (value is null || value is double d && double.IsNaN(d))
        {
            target.ClearValue(property);
        }
        else
        {
            target.SetValue(property, value);
        }
    }

    private static void Track(FrameworkElement element, Action<FrameworkElement, double> apply)
    {
        if (element.GetValue(TrackerProperty) is not Tracker tracker)
        {
            tracker = new Tracker(element);
            element.SetValue(TrackerProperty, tracker);
        }

        tracker.Add(apply);
    }

    private static readonly DependencyProperty TrackerProperty =
        DependencyProperty.RegisterAttached("Tracker", typeof(object), typeof(ResponsiveHelper), new PropertyMetadata(null));

    private sealed class Tracker
    {
        private readonly FrameworkElement _element;
        private readonly List<Action<FrameworkElement, double>> _actions = new();
        private Action _unsubscribe;

        public Tracker(FrameworkElement element)
        {
            _element = element;
            element.Loaded += (_, _) => Attach();
            element.Unloaded += (_, _) => Detach();

            if (element.IsLoaded)
            {
                Attach();
            }
        }

        public void Add(Action<FrameworkElement, double> apply)
        {
            if (!_actions.Contains(apply))
            {
                _actions.Add(apply);
            }

            Update();
        }

        private void Attach()
        {
            _unsubscribe ??= Subscribe(_element, Update);
            Update();
        }

        private void Detach()
        {
            _unsubscribe?.Invoke();
            _unsubscribe = null;
        }

        private void Update()
        {
            double width = GetWidth(_element);
            if (width <= 0)
            {
                return; // 아직 창에 붙지 않았다. Loaded에서 다시 적용된다.
            }

            foreach (var apply in _actions)
            {
                apply(_element, width);
            }

            AppWidth.Raise(width);
        }
    }

#if WPF
    private static double GetWidth(FrameworkElement element) => Window.GetWindow(element)?.ActualWidth ?? 0;

    private static Action Subscribe(FrameworkElement element, Action onChanged)
    {
        var window = Window.GetWindow(element);
        if (window == null)
        {
            return null;
        }

        SizeChangedEventHandler handler = (_, _) => onChanged();
        window.SizeChanged += handler;
        return () => window.SizeChanged -= handler;
    }
#elif WINUI3 || UNO
    // WinUI3/Uno 데스크톱에서는 Window.Current가 null이므로 요소의 XamlRoot를 쓴다.
    private static double GetWidth(FrameworkElement element) => element.XamlRoot?.Size.Width ?? 0;

    private static Action Subscribe(FrameworkElement element, Action onChanged)
    {
        var root = element.XamlRoot;
        if (root == null)
        {
            return null;
        }

        TypedEventHandler<XamlRoot, XamlRootChangedEventArgs> handler = (_, _) => onChanged();
        root.Changed += handler;
        return () => root.Changed -= handler;
    }
#else // OPENSILVER, UWP
    private static double GetWidth(FrameworkElement element) => Window.Current?.Bounds.Width ?? 0;

    private static Action Subscribe(FrameworkElement element, Action onChanged)
    {
        var window = Window.Current;
        if (window == null)
        {
            return null;
        }

        WindowSizeChangedEventHandler handler = (_, _) => onChanged();
        window.SizeChanged += handler;
        return () => window.SizeChanged -= handler;
    }
#endif

    /// <summary>
    /// FrameworkElement가 아니어서 자기 창을 알 수 없는 대상(DoubleAnimation 등)을 위한 앱 단위 너비.
    /// 대상은 약한 참조로만 보관하므로 이 목록이 대상을 붙잡아 두지 않는다.
    ///
    /// 너비 출처: WPF는 MainWindow, OpenSilver/UWP는 Window.Current를 직접 구독한다.
    /// WinUI3/Uno는 앱 전역 창이 없으므로, 화면에 있는 Responsive* 요소가 감지한 창 너비를 이어받는다.
    /// </summary>
    internal static class AppWidth
    {
        private static readonly List<(WeakReference<DependencyObject> Target, Action<DependencyObject, double> Apply)> _targets = new();
        private static double _last;
        private static bool _hooked;

        public static void Track(DependencyObject target, Action<DependencyObject, double> apply)
        {
            if (!_targets.Exists(t => t.Target.TryGetTarget(out var o) && ReferenceEquals(o, target)))
            {
                _targets.Add((new WeakReference<DependencyObject>(target), apply));
            }

            Hook();
            if (_last > 0)
            {
                apply(target, _last);
            }
        }

        public static void Raise(double width)
        {
            _last = width;
            _targets.RemoveAll(t => !t.Target.TryGetTarget(out _));

            foreach (var (target, apply) in _targets.ToArray())
            {
                if (target.TryGetTarget(out var o))
                {
                    apply(o, width);
                }
            }
        }

        private static void Hook()
        {
            if (_hooked)
            {
                return;
            }
#if WPF
            var window = Application.Current?.MainWindow;
            if (window == null)
            {
                return; // MainWindow가 생긴 뒤 다음 Track 호출에서 다시 시도한다.
            }

            _hooked = true;
            window.SizeChanged += (_, _) => Raise(window.ActualWidth);
            if (window.ActualWidth > 0)
            {
                _last = window.ActualWidth;
            }
#elif OPENSILVER || UWP
            var window = Window.Current;
            if (window == null)
            {
                return;
            }

            _hooked = true;
            window.SizeChanged += (_, _) => Raise(window.Bounds.Width);
            _last = window.Bounds.Width;
#else
            _hooked = true;
#endif
        }
    }
}
