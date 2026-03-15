using System.Windows;
using System.Windows.Controls;

namespace Jamesnet.Platform.Wpf.Responsive;

public static class ResponsiveFontWeight
{
    public static readonly DependencyProperty DesktopFontWeightProperty =
        DependencyProperty.RegisterAttached("DesktopFontWeight", typeof(FontWeight?), typeof(ResponsiveFontWeight),
            new PropertyMetadata(null, OnPropertyChanged));
    public static FontWeight? GetDesktopFontWeight(DependencyObject obj) => (FontWeight?)obj.GetValue(DesktopFontWeightProperty);
    public static void SetDesktopFontWeight(DependencyObject obj, FontWeight? value) => obj.SetValue(DesktopFontWeightProperty, value);

    public static readonly DependencyProperty MobileFontWeightProperty =
        DependencyProperty.RegisterAttached("MobileFontWeight", typeof(FontWeight?), typeof(ResponsiveFontWeight),
            new PropertyMetadata(null, OnPropertyChanged));
    public static FontWeight? GetMobileFontWeight(DependencyObject obj) => (FontWeight?)obj.GetValue(MobileFontWeightProperty);
    public static void SetMobileFontWeight(DependencyObject obj, FontWeight? value) => obj.SetValue(MobileFontWeightProperty, value);

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
            ? GetMobileFontWeight(element)
            : GetDesktopFontWeight(element);

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
