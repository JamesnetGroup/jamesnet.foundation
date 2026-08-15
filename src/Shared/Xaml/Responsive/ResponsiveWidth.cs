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
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop Width를 적용하는
/// 플랫폼 중립 attached property. 소스는 이 파일 한 벌이고 각 flavor가 컴파일한다.
/// </summary>
public static class ResponsiveWidth
{
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(double), typeof(ResponsiveWidth),
            new PropertyMetadata(double.NaN, OnWidthChanged));

    public static double GetDesktop(DependencyObject obj) => (double)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, double value) => obj.SetValue(DesktopProperty, value);

    public static readonly DependencyProperty MobileWidthProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(double), typeof(ResponsiveWidth),
            new PropertyMetadata(double.NaN, OnWidthChanged));

    public static double GetMobile(DependencyObject obj) => (double)obj.GetValue(MobileWidthProperty);
    public static void SetMobile(DependencyObject obj, double value) => obj.SetValue(MobileWidthProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveWidth),
            new PropertyMetadata(768.0, OnWidthChanged));

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

#if WPF
        // WPF는 Window.GetWindow 기반의 ResponsiveHelper가 구독/해제를 관리한다.
        ResponsiveHelper.Register(element, UpdateWidth);
#else
        if (!IsHandlerRegistered(element))
        {
            Window.Current.SizeChanged += (s, args) => UpdateWidth(element);
            element.Loaded += (s, args) => UpdateWidth(element);
            RegisterHandler(element);
        }
        UpdateWidth(element);
#endif
    }

#if WPF
    private static void UpdateWidth(FrameworkElement element, double windowWidth)
    {
        ApplyWidth(element, windowWidth);
    }
#else
    private static void UpdateWidth(FrameworkElement element)
    {
        ApplyWidth(element, Window.Current.Bounds.Width);
    }

    // 이벤트 중복 등록 방지
    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveWidth),
            new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
#endif

    private static void ApplyWidth(FrameworkElement element, double windowWidth)
    {
        double breakpoint = GetBreakpoint(element);
        double defaultWidth = GetDesktop(element);
        double mobileWidth = GetMobile(element);

        if (windowWidth <= breakpoint)
        {
            if (!double.IsNaN(mobileWidth))
                element.Width = mobileWidth;
            else
                element.ClearValue(FrameworkElement.WidthProperty);
        }
        else
        {
            if (!double.IsNaN(defaultWidth))
                element.Width = defaultWidth;
            else
                element.ClearValue(FrameworkElement.WidthProperty);
        }
    }
}
