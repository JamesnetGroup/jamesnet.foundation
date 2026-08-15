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
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop Height를 적용하는
/// 플랫폼 중립 attached property. 소스는 이 파일 한 벌이고 각 flavor가 컴파일한다.
/// </summary>
public static class ResponsiveHeight
{
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(double), typeof(ResponsiveHeight),
            new PropertyMetadata(double.NaN, OnHeightChanged));

    public static double GetDesktop(DependencyObject obj) => (double)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, double value) => obj.SetValue(DesktopProperty, value);

    public static readonly DependencyProperty MobileProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(double), typeof(ResponsiveHeight),
            new PropertyMetadata(double.NaN, OnHeightChanged));

    public static double GetMobile(DependencyObject obj) => (double)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, double value) => obj.SetValue(MobileProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveHeight),
            new PropertyMetadata(768.0, OnHeightChanged));

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

#if WPF
        // WPF는 Window.GetWindow 기반의 ResponsiveHelper가 구독/해제를 관리한다.
        ResponsiveHelper.Register(element, UpdateHeight);
#else
        if (!IsHandlerRegistered(element))
        {
            Window.Current.SizeChanged += (s, args) => UpdateHeight(element);
            element.Loaded += (s, args) => UpdateHeight(element);
            RegisterHandler(element);
        }
        UpdateHeight(element);
#endif
    }

#if WPF
    private static void UpdateHeight(FrameworkElement element, double windowWidth)
    {
        ApplyHeight(element, windowWidth);
    }
#else
    private static void UpdateHeight(FrameworkElement element)
    {
        ApplyHeight(element, Window.Current.Bounds.Width);
    }

    // 이벤트 중복 등록 방지
    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveHeight),
            new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
#endif

    private static void ApplyHeight(FrameworkElement element, double windowWidth)
    {
        double breakpoint = GetBreakpoint(element);
        double defaultHeight = GetDesktop(element);
        double mobileHeight = GetMobile(element);

        if (windowWidth <= breakpoint)
        {
            if (!double.IsNaN(mobileHeight))
                element.Height = mobileHeight;
            else
                element.ClearValue(FrameworkElement.HeightProperty);
        }
        else
        {
            if (!double.IsNaN(defaultHeight))
                element.Height = defaultHeight;
            else
                element.ClearValue(FrameworkElement.HeightProperty);
        }
    }
}
