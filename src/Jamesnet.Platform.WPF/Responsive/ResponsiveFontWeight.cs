using System.Windows;
using System.Windows.Controls;

namespace Jamesnet.Platform.Wpf.Responsive;

public static class ResponsiveFontWeight
{
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(FontWeight?), typeof(ResponsiveFontWeight),
            new PropertyMetadata(null, OnPropertyChanged));
    public static FontWeight? GetDesktop(DependencyObject obj) => (FontWeight?)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, FontWeight? value) => obj.SetValue(DesktopProperty, value);

    public static readonly DependencyProperty MobileProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(FontWeight?), typeof(ResponsiveFontWeight),
            new PropertyMetadata(null, OnPropertyChanged));
    public static FontWeight? GetMobile(DependencyObject obj) => (FontWeight?)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, FontWeight? value) => obj.SetValue(MobileProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveFontWeight),
            new PropertyMetadata(768.0, OnPropertyChanged));
    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement element)
            ResponsiveHelper.Register(element, UpdateFontWeight);
    }

    private static void UpdateFontWeight(FrameworkElement element, double windowWidth)
    {
        double breakpoint = GetBreakpoint(element);
        var target = windowWidth <= breakpoint
            ? GetMobile(element)
            : GetDesktop(element);

        if (element is Control c)
        {
            if (target.HasValue) c.FontWeight = target.Value;
            else c.ClearValue(Control.FontWeightProperty);
        }
        else if (element is TextBlock tb)
        {
            if (target.HasValue) tb.FontWeight = target.Value;
            else tb.ClearValue(TextBlock.FontWeightProperty);
        }
    }
}
