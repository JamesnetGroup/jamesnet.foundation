#if WINUI3 || UNO
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
#else // WPF, OPENSILVER
using System.Windows;
using System.Windows.Media.Animation;
#endif

namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// 창 너비가 Breakpoint 이하이면 MobileTo, 아니면 DesktopTo를 DoubleAnimation.To에 적용하는
/// 플랫폼 중립 attached property. 소스는 이 파일 한 벌이고 각 flavor가 컴파일한다.
///
/// 주의:
/// - 기존 WPF판 프로퍼티명(Desktop/Mobile)은 OpenSilver판(DesktopTo/MobileTo)으로 통일했다. (v10 breaking change)
/// - 대상이 FrameworkElement가 아니라 DoubleAnimation이라 WPF의 ResponsiveHelper를 쓸 수 없다.
///   그래서 WPF 분기는 기존 WPF 구현처럼 Application.Current.MainWindow를 캐시해 구독하는 방식을 유지하고,
///   나머지 플랫폼은 Window.Current.SizeChanged 패턴을 쓴다.
/// </summary>
public static class ResponsiveAnimationTo
{
    public static readonly DependencyProperty DesktopToProperty =
        DependencyProperty.RegisterAttached("DesktopTo", typeof(double), typeof(ResponsiveAnimationTo),
            new PropertyMetadata(double.NaN, OnToValueChanged));

    public static double GetDesktopTo(DependencyObject obj) => (double)obj.GetValue(DesktopToProperty);
    public static void SetDesktopTo(DependencyObject obj, double value) => obj.SetValue(DesktopToProperty, value);

    public static readonly DependencyProperty MobileToProperty =
        DependencyProperty.RegisterAttached("MobileTo", typeof(double), typeof(ResponsiveAnimationTo),
            new PropertyMetadata(double.NaN, OnToValueChanged));

    public static double GetMobileTo(DependencyObject obj) => (double)obj.GetValue(MobileToProperty);
    public static void SetMobileTo(DependencyObject obj, double value) => obj.SetValue(MobileToProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveAnimationTo),
            new PropertyMetadata(768.0, OnToValueChanged));

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

#if WPF
    // DoubleAnimation은 FrameworkElement가 아니므로 Window.GetWindow를 쓸 수 없다.
    // MainWindow를 attached property에 캐시하고 SizeChanged 핸들러를 함께 보관한다.
    private static readonly DependencyProperty CachedWindowProperty =
        DependencyProperty.RegisterAttached("CachedWindow", typeof(Window), typeof(ResponsiveAnimationTo),
            new PropertyMetadata(null));

    private static readonly DependencyProperty HandlerProperty =
        DependencyProperty.RegisterAttached("Handler", typeof(SizeChangedEventHandler), typeof(ResponsiveAnimationTo),
            new PropertyMetadata(null));

    private static void OnToValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DoubleAnimation animation)
        {
            return;
        }

        Window window = (Window)animation.GetValue(CachedWindowProperty);

        if (window == null)
        {
            window = Application.Current?.MainWindow;
            if (window == null)
            {
                return;
            }

            animation.SetValue(CachedWindowProperty, window);

            SizeChangedEventHandler handler = (s, args) => UpdateToValue(animation);
            animation.SetValue(HandlerProperty, handler);
            window.SizeChanged += handler;
        }

        UpdateToValue(animation);
    }

    private static void UpdateToValue(DoubleAnimation animation)
    {
        Window window = (Window)animation.GetValue(CachedWindowProperty);
        if (window == null)
        {
            return;
        }

        ApplyToValue(animation, window.ActualWidth);
    }
#else
    private static void OnToValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DoubleAnimation animation)
        {
            return;
        }

        if (!IsHandlerRegistered(animation))
        {
            Window.Current.SizeChanged += (s, args) => UpdateToValue(animation);
            RegisterHandler(animation);
        }
        UpdateToValue(animation);
    }

    private static void UpdateToValue(DoubleAnimation animation)
    {
        ApplyToValue(animation, Window.Current.Bounds.Width);
    }

    // 이벤트 중복 등록 방지
    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveAnimationTo),
            new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
#endif

    private static void ApplyToValue(DoubleAnimation animation, double windowWidth)
    {
        double breakpoint = GetBreakpoint(animation);
        double desktopToValue = GetDesktopTo(animation);
        double mobileToValue = GetMobileTo(animation);

        animation.To = windowWidth <= breakpoint
            ? (!double.IsNaN(mobileToValue) ? mobileToValue : desktopToValue)
            : (!double.IsNaN(desktopToValue) ? desktopToValue : mobileToValue);
    }
}
