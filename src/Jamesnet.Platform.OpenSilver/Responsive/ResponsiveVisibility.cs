using System.Windows;

namespace Jamesnet.Platform.OpenSilver.Responsive;

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
        if (d is FrameworkElement element)
        {
            if (!IsHandlerRegistered(element))
            {
                Window.Current.SizeChanged += (s, args) => UpdateVisibility(element);
                element.Loaded += (s, args) => UpdateVisibility(element);
                RegisterHandler(element);
            }
            UpdateVisibility(element);
        }
    }

    private static void UpdateVisibility(FrameworkElement element)
    {
        double windowWidth = Window.Current.Bounds.Width;
        double breakpoint = GetBreakpoint(element);
        Visibility newVisibility = windowWidth <= breakpoint ? GetMobile(element) : GetDesktop(element);
        element.Visibility = newVisibility;
    }

    // 이벤트 중복 등록 방지
    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveVisibility),
            new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
}