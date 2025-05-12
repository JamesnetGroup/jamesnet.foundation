using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Jamesnet.Platform.OpenSilver.Responsive;

public static class ResponsiveMargin
{
    // DefaultMargin 부착 속성
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(Thickness), typeof(ResponsiveMargin),
            new PropertyMetadata(new Thickness(0), OnMarginChanged));

    public static Thickness GetDesktop(DependencyObject obj) => (Thickness)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, Thickness value) => obj.SetValue(DesktopProperty, value);

    // MobileMargin 부착 속성
    public static readonly DependencyProperty MobileProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(Thickness), typeof(ResponsiveMargin),
            new PropertyMetadata(new Thickness(0), OnMarginChanged));

    public static Thickness GetMobile(DependencyObject obj) => (Thickness)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, Thickness value) => obj.SetValue(MobileProperty, value);

    // Breakpoint 부착 속성
    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveMargin),
            new PropertyMetadata(768.0, OnMarginChanged));

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement element)
        {
            // 창 크기 변경 이벤트 구독 (한 번만)
            if (!IsHandlerRegistered(element))
            {
                Window.Current.SizeChanged += (s, args) => UpdateMargin(element);
                element.Loaded += (s, args) => UpdateMargin(element);
                RegisterHandler(element);
            }
            UpdateMargin(element);
        }
    }

    private static void UpdateMargin(FrameworkElement element)
    {
        double windowWidth = Window.Current.Bounds.Width;
        double breakpoint = GetBreakpoint(element);
        Thickness newMargin = windowWidth <= breakpoint ? GetMobile(element) : GetDesktop(element);
        element.Margin = newMargin;
    }

    // 이벤트 중복 등록 방지
    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveMargin), new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
}
