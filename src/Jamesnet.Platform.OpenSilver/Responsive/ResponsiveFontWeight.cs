using System.Windows;
using System.Windows.Controls;

namespace Jamesnet.Platform.OpenSilver.Responsive;

public static class ResponsiveFontWeight
{
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(FontWeight?), typeof(ResponsiveFontWeight),
            new PropertyMetadata(null, OnFontWeightChanged));

    public static FontWeight? GetDesktop(DependencyObject obj) => (FontWeight?)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, FontWeight? value) => obj.SetValue(DesktopProperty, value);

    public static readonly DependencyProperty MobileProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(FontWeight?), typeof(ResponsiveFontWeight),
            new PropertyMetadata(null, OnFontWeightChanged));

    public static FontWeight? GetMobile(DependencyObject obj) => (FontWeight?)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, FontWeight? value) => obj.SetValue(MobileProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveFontWeight),
            new PropertyMetadata(768.0, OnFontWeightChanged));

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnFontWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Control || d is TextBlock)
        {
            if (!IsHandlerRegistered(d))
            {
                Window.Current.SizeChanged += (s, args) => UpdateFontWeight(d);
                if (d is FrameworkElement fe)
                {
                    fe.Loaded += (s, args) => UpdateFontWeight(d);
                }
                RegisterHandler(d);
            }
            UpdateFontWeight(d);
        }
    }

    private static void UpdateFontWeight(DependencyObject element)
    {
        double windowWidth = Window.Current.Bounds.Width;
        double breakpoint = GetBreakpoint(element);
        var desktopWeight = GetDesktop(element);
        var mobileWeight = GetMobile(element);

        if (element is Control control)
        {
            if (windowWidth <= breakpoint)
            {
                if (mobileWeight.HasValue)
                    control.FontWeight = mobileWeight.Value;
                else
                    control.ClearValue(Control.FontWeightProperty);
            }
            else
            {
                if (desktopWeight.HasValue)
                    control.FontWeight = desktopWeight.Value;
                else
                    control.ClearValue(Control.FontWeightProperty);
            }
        }
        else if (element is TextBlock textBlock)
        {
            if (windowWidth <= breakpoint)
            {
                if (mobileWeight.HasValue)
                    textBlock.FontWeight = mobileWeight.Value;
                else
                    textBlock.ClearValue(TextBlock.FontWeightProperty);
            }
            else
            {
                if (desktopWeight.HasValue)
                    textBlock.FontWeight = desktopWeight.Value;
                else
                    textBlock.ClearValue(TextBlock.FontWeightProperty);
            }
        }
    }

    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveFontWeight),
            new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
}
