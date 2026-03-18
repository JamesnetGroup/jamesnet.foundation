using System.Windows;

namespace Jamesnet.Platform.Wpf.Responsive;

public static class ResponsiveVisibility
{
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(Visibility), typeof(ResponsiveVisibility),
            new PropertyMetadata(Visibility.Visible, OnPropertyChanged));
    public static Visibility GetDesktop(DependencyObject obj) => (Visibility)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, Visibility value) => obj.SetValue(DesktopProperty, value);

    public static readonly DependencyProperty MobileProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(Visibility), typeof(ResponsiveVisibility),
            new PropertyMetadata(Visibility.Collapsed, OnPropertyChanged));
    public static Visibility GetMobile(DependencyObject obj) => (Visibility)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, Visibility value) => obj.SetValue(MobileProperty, value);

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
            ? GetMobile(element)
            : GetDesktop(element);
    }
}
