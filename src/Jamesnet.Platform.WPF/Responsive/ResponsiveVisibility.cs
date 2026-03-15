using System.Windows;

namespace Jamesnet.Platform.Wpf.Responsive;

public static class ResponsiveVisibility
{
    public static readonly DependencyProperty DesktopVisibilityProperty =
        DependencyProperty.RegisterAttached("DesktopVisibility", typeof(Visibility), typeof(ResponsiveVisibility),
            new PropertyMetadata(Visibility.Visible, OnPropertyChanged));
    public static Visibility GetDesktopVisibility(DependencyObject obj) => (Visibility)obj.GetValue(DesktopVisibilityProperty);
    public static void SetDesktopVisibility(DependencyObject obj, Visibility value) => obj.SetValue(DesktopVisibilityProperty, value);

    public static readonly DependencyProperty MobileVisibilityProperty =
        DependencyProperty.RegisterAttached("MobileVisibility", typeof(Visibility), typeof(ResponsiveVisibility),
            new PropertyMetadata(Visibility.Collapsed, OnPropertyChanged));
    public static Visibility GetMobileVisibility(DependencyObject obj) => (Visibility)obj.GetValue(MobileVisibilityProperty);
    public static void SetMobileVisibility(DependencyObject obj, Visibility value) => obj.SetValue(MobileVisibilityProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveVisibility),
            new PropertyMetadata(768.0, OnPropertyChanged));
    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement element)
            ResponsiveHelper.Register(element, UpdateVisibility);
    }

    private static void UpdateVisibility(FrameworkElement element, double windowWidth)
    {
        double breakpoint = GetBreakpoint(element);
        element.Visibility = windowWidth <= breakpoint
            ? GetMobileVisibility(element)
            : GetDesktopVisibility(element);
    }
}
