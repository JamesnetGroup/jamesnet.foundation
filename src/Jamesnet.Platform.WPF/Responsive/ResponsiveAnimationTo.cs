using System.Windows;
using System.Windows.Media.Animation;

namespace Jamesnet.Platform.Wpf.Responsive;

public static class ResponsiveAnimationTo
{
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(double), typeof(ResponsiveAnimationTo),
            new PropertyMetadata(double.NaN, OnPropertyChanged));
    public static double GetDesktop(DependencyObject obj) => (double)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, double value) => obj.SetValue(DesktopProperty, value);

    public static readonly DependencyProperty MobileProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(double), typeof(ResponsiveAnimationTo),
            new PropertyMetadata(double.NaN, OnPropertyChanged));
    public static double GetMobile(DependencyObject obj) => (double)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, double value) => obj.SetValue(MobileProperty, value);

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
        double desktop = GetDesktop(animation);
        double mobile = GetMobile(animation);

        animation.To = windowWidth <= breakpoint
            ? (!double.IsNaN(mobile) ? mobile : desktop)
            : (!double.IsNaN(desktop) ? desktop : mobile);
    }
}
