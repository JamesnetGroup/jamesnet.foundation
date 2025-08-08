using System.Windows;
using System.Windows.Media.Animation;

namespace Jamesnet.Platform.OpenSilver.Responsive;

public static class ResponsiveAnimationTo
{
    public static readonly DependencyProperty DesktopToProperty = DependencyProperty.RegisterAttached("DesktopTo", typeof(double), typeof(ResponsiveAnimationTo), new PropertyMetadata(double.NaN, OnToValueChanged));
    public static readonly DependencyProperty MobileToProperty = DependencyProperty.RegisterAttached("MobileTo", typeof(double), typeof(ResponsiveAnimationTo), new PropertyMetadata(double.NaN, OnToValueChanged));
    public static readonly DependencyProperty BreakpointProperty = DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveAnimationTo), new PropertyMetadata(768.0, OnToValueChanged));
    private static readonly DependencyProperty IsHandlerRegisteredProperty = DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveAnimationTo), new PropertyMetadata(false));
    
    public static double GetDesktopTo(DependencyObject obj) => (double)obj.GetValue(DesktopToProperty);
    public static void SetDesktopTo(DependencyObject obj, double value) => obj.SetValue(DesktopToProperty, value);
    
    public static double GetMobileTo(DependencyObject obj) => (double)obj.GetValue(MobileToProperty);
    public static void SetMobileTo(DependencyObject obj, double value) => obj.SetValue(MobileToProperty, value);

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);

    private static void OnToValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DoubleAnimation animation)
        {
            if (!IsHandlerRegistered(animation))
            {
                Window.Current.SizeChanged += (s, args) => UpdateToValue(animation);
                RegisterHandler(animation);
            }
            UpdateToValue(animation);
        }
    }

    private static void UpdateToValue(DoubleAnimation animation)
    {
        double windowWidth = Window.Current.Bounds.Width;
        double breakpoint = GetBreakpoint(animation);
        double desktopToValue = GetDesktopTo(animation);
        double mobileToValue = GetMobileTo(animation);

        if (windowWidth <= breakpoint)
        {
            if (!double.IsNaN(mobileToValue))
            {
                animation.To = mobileToValue;
            }
            else if (!double.IsNaN(desktopToValue))
            {
                animation.To = desktopToValue;
            }
        }
        else
        {
            if (!double.IsNaN(desktopToValue))
            {
                animation.To = desktopToValue;
            }
            else if (!double.IsNaN(mobileToValue))
            {
                animation.To = mobileToValue;
            }
        }
    }
}