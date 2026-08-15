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
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop Visibility를 적용하는
/// 플랫폼 중립 attached property. 소스는 이 파일 한 벌이고 각 flavor가 컴파일한다.
/// </summary>
public static class ResponsiveVisibility
{
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(Visibility), typeof(ResponsiveVisibility),
            new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));

    public static Visibility GetDesktop(DependencyObject obj) => (Visibility)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, Visibility value) => obj.SetValue(DesktopProperty, value);

    public static readonly DependencyProperty MobileProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(Visibility), typeof(ResponsiveVisibility),
            new PropertyMetadata(Visibility.Collapsed, OnVisibilityChanged));

    public static Visibility GetMobile(DependencyObject obj) => (Visibility)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, Visibility value) => obj.SetValue(MobileProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveVisibility),
            new PropertyMetadata(768.0, OnVisibilityChanged));

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

#if WPF
        // WPF는 Window.GetWindow 기반의 ResponsiveHelper가 구독/해제를 관리한다.
        ResponsiveHelper.Register(element, UpdateVisibility);
#else
        if (!IsHandlerRegistered(element))
        {
            Window.Current.SizeChanged += (s, args) => UpdateVisibility(element);
            element.Loaded += (s, args) => UpdateVisibility(element);
            RegisterHandler(element);
        }
        UpdateVisibility(element);
#endif
    }

#if WPF
    private static void UpdateVisibility(FrameworkElement element, double windowWidth)
    {
        double breakpoint = GetBreakpoint(element);
        element.Visibility = windowWidth <= breakpoint ? GetMobile(element) : GetDesktop(element);
    }
#else
    private static void UpdateVisibility(FrameworkElement element)
    {
        double windowWidth = Window.Current.Bounds.Width;
        double breakpoint = GetBreakpoint(element);
        element.Visibility = windowWidth <= breakpoint ? GetMobile(element) : GetDesktop(element);
    }

    // 이벤트 중복 등록 방지
    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveVisibility),
            new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
#endif
}
