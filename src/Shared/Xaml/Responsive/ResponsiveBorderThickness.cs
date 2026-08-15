#if WINUI3 || UNO
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else // WPF, OPENSILVER
using System.Windows;
using System.Windows.Controls;
#endif
#if WPF
using Jamesnet.Platform.Wpf.Responsive;
#endif

namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop BorderThickness를 적용하는
/// 플랫폼 중립 attached property. Control과 Border에 붙일 수 있다.
/// 소스는 이 파일 한 벌이고 각 flavor가 컴파일한다.
/// </summary>
public static class ResponsiveBorderThickness
{
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(Thickness), typeof(ResponsiveBorderThickness),
            new PropertyMetadata(new Thickness(0), OnBorderThicknessChanged));

    public static Thickness GetDesktop(DependencyObject obj) => (Thickness)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, Thickness value) => obj.SetValue(DesktopProperty, value);

    public static readonly DependencyProperty MobileProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(Thickness), typeof(ResponsiveBorderThickness),
            new PropertyMetadata(new Thickness(0), OnBorderThicknessChanged));

    public static Thickness GetMobile(DependencyObject obj) => (Thickness)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, Thickness value) => obj.SetValue(MobileProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveBorderThickness),
            new PropertyMetadata(768.0, OnBorderThicknessChanged));

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element || (element is not Control && element is not Border))
        {
            return;
        }

#if WPF
        // WPF는 Window.GetWindow 기반의 ResponsiveHelper가 구독/해제를 관리한다.
        ResponsiveHelper.Register(element, UpdateBorderThickness);
#else
        if (!IsHandlerRegistered(element))
        {
            Window.Current.SizeChanged += (s, args) => UpdateBorderThickness(element);
            element.Loaded += (s, args) => UpdateBorderThickness(element);
            RegisterHandler(element);
        }
        UpdateBorderThickness(element);
#endif
    }

#if WPF
    private static void UpdateBorderThickness(FrameworkElement element, double windowWidth)
    {
        ApplyBorderThickness(element, windowWidth);
    }
#else
    private static void UpdateBorderThickness(FrameworkElement element)
    {
        ApplyBorderThickness(element, Window.Current.Bounds.Width);
    }

    // 이벤트 중복 등록 방지
    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveBorderThickness),
            new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
#endif

    private static void ApplyBorderThickness(DependencyObject element, double windowWidth)
    {
        double breakpoint = GetBreakpoint(element);
        Thickness borderThickness = windowWidth <= breakpoint ? GetMobile(element) : GetDesktop(element);

        if (element is Control control)
        {
            control.BorderThickness = borderThickness;
        }
        else if (element is Border border)
        {
            border.BorderThickness = borderThickness;
        }
    }
}
