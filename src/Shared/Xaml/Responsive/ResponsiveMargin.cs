#if WINUI3 || UNO
using Microsoft.UI.Xaml;
#elif UWP
using Windows.UI.Xaml;
#else // WPF, OPENSILVER
using System.Windows;
#endif
#if WPF
using Jamesnet.Platform.Wpf.Responsive;
#endif

namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop Margin을 적용하는
/// 플랫폼 중립 attached property. 소스는 이 파일 한 벌이고 각 flavor가 컴파일한다.
/// </summary>
public static class ResponsiveMargin
{
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(Thickness), typeof(ResponsiveMargin),
            new PropertyMetadata(new Thickness(0), OnMarginChanged));

    public static Thickness GetDesktop(DependencyObject obj) => (Thickness)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, Thickness value) => obj.SetValue(DesktopProperty, value);

    public static readonly DependencyProperty MobileProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(Thickness), typeof(ResponsiveMargin),
            new PropertyMetadata(new Thickness(0), OnMarginChanged));

    public static Thickness GetMobile(DependencyObject obj) => (Thickness)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, Thickness value) => obj.SetValue(MobileProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveMargin),
            new PropertyMetadata(768.0, OnMarginChanged));

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

#if WPF
        // WPF는 Window.GetWindow 기반의 ResponsiveHelper가 구독/해제를 관리한다.
        ResponsiveHelper.Register(element, UpdateMargin);
#else
        if (!IsHandlerRegistered(element))
        {
            Window.Current.SizeChanged += (s, args) => UpdateMargin(element);
            element.Loaded += (s, args) => UpdateMargin(element);
            RegisterHandler(element);
        }
        UpdateMargin(element);
#endif
    }

#if WPF
    private static void UpdateMargin(FrameworkElement element, double windowWidth)
    {
        double breakpoint = GetBreakpoint(element);
        element.Margin = windowWidth <= breakpoint ? GetMobile(element) : GetDesktop(element);
    }
#else
    private static void UpdateMargin(FrameworkElement element)
    {
        double windowWidth = Window.Current.Bounds.Width;
        double breakpoint = GetBreakpoint(element);
        element.Margin = windowWidth <= breakpoint ? GetMobile(element) : GetDesktop(element);
    }

    // 이벤트 중복 등록 방지
    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveMargin),
            new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
#endif
}
