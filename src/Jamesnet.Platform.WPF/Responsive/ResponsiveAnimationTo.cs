using System.Windows;
using System.Windows.Media.Animation;

namespace Jamesnet.Platform.Wpf.Responsive;

public static class ResponsiveAnimationTo
{
    public static readonly DependencyProperty DesktopToProperty =
        DependencyProperty.RegisterAttached("DesktopTo", typeof(double), typeof(ResponsiveAnimationTo),
            new PropertyMetadata(double.NaN, OnPropertyChanged));
    public static double GetDesktopTo(DependencyObject obj) => (double)obj.GetValue(DesktopToProperty);
    public static void SetDesktopTo(DependencyObject obj, double value) => obj.SetValue(DesktopToProperty, value);

    public static readonly DependencyProperty MobileToProperty =
        DependencyProperty.RegisterAttached("MobileTo", typeof(double), typeof(ResponsiveAnimationTo),
            new PropertyMetadata(double.NaN, OnPropertyChanged));
    public static double GetMobileTo(DependencyObject obj) => (double)obj.GetValue(MobileToProperty);
    public static void SetMobileTo(DependencyObject obj, double value) => obj.SetValue(MobileToProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveAnimationTo),
            new PropertyMetadata(768.0, OnPropertyChanged));
    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static readonly DependencyProperty CachedWindowProperty =
        DependencyProperty.RegisterAttached("CachedWindow", typeof(Window), typeof(ResponsiveAnimationTo),
            new PropertyMetadata(null));

    private static readonly DependencyProperty HandlerProperty =
        DependencyProperty.RegisterAttached("Handler", typeof(SizeChangedEventHandler), typeof(ResponsiveAnimationTo),
            new PropertyMetadata(null));

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DoubleAnimation animation) return;

        Window window = (Window)animation.GetValue(CachedWindowProperty);

        if (window == null)
        {
            window = Application.Current?.MainWindow;
            if (window == null) return;

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
        if (window == null) return;

        double windowWidth = window.ActualWidth;
        double breakpoint = GetBreakpoint(animation);
        double desktopTo = GetDesktopTo(animation);
        double mobileTo = GetMobileTo(animation);

        animation.To = windowWidth <= breakpoint
            ? (!double.IsNaN(mobileTo) ? mobileTo : desktopTo)
            : (!double.IsNaN(desktopTo) ? desktopTo : mobileTo);
    }
}
