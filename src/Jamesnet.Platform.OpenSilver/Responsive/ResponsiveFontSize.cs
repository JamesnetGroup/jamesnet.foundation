using System.Windows;
using System.Windows.Controls;

namespace Jamesnet.Platform.OpenSilver.Responsive
{
    public static class ResponsiveFontSize
    {
        public static readonly DependencyProperty DesktopProperty =
            DependencyProperty.RegisterAttached("Desktop", typeof(double), typeof(ResponsiveFontSize),
                new PropertyMetadata(double.NaN, OnFontSizeChanged));

        public static double GetDesktop(DependencyObject obj) => (double)obj.GetValue(DesktopProperty);
        public static void SetDesktop(DependencyObject obj, double value) => obj.SetValue(DesktopProperty, value);

        public static readonly DependencyProperty MobileProperty =
            DependencyProperty.RegisterAttached("Mobile", typeof(double), typeof(ResponsiveFontSize),
                new PropertyMetadata(double.NaN, OnFontSizeChanged));

        public static double GetMobile(DependencyObject obj) => (double)obj.GetValue(MobileProperty);
        public static void SetMobile(DependencyObject obj, double value) => obj.SetValue(MobileProperty, value);

        public static readonly DependencyProperty BreakpointProperty =
            DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveFontSize),
                new PropertyMetadata(768.0, OnFontSizeChanged));

        public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
        public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control || d is TextBlock)
            {
                if (!IsHandlerRegistered(d))
                {
                    Window.Current.SizeChanged += (s, args) => UpdateFontSize(d);
                    if (d is FrameworkElement element)
                    {
                        element.Loaded += (s, args) => UpdateFontSize(d);
                    }
                    RegisterHandler(d);
                }
                UpdateFontSize(d);
            }
        }

        private static void UpdateFontSize(DependencyObject element)
        {
            double windowWidth = Window.Current.Bounds.Width; // Breakpoint 비교를 위해 너비 사용
            double breakpoint = GetBreakpoint(element);
            double defaultFontSize = GetDesktop(element);
            double mobileFontSize = GetMobile(element);

            if (element is Control control)
            {
                if (windowWidth <= breakpoint)
                {
                    if (!double.IsNaN(mobileFontSize))
                        control.FontSize = mobileFontSize;
                    else
                        control.ClearValue(Control.FontSizeProperty);
                }
                else
                {
                    if (!double.IsNaN(defaultFontSize))
                        control.FontSize = defaultFontSize;
                    else
                        control.ClearValue(Control.FontSizeProperty);
                }
            }
            else if (element is TextBlock textBlock)
            {
                if (windowWidth <= breakpoint)
                {
                    if (!double.IsNaN(mobileFontSize))
                        textBlock.FontSize = mobileFontSize;
                    else
                        textBlock.ClearValue(TextBlock.FontSizeProperty);
                }
                else
                {
                    if (!double.IsNaN(defaultFontSize))
                        textBlock.FontSize = defaultFontSize;
                    else
                        textBlock.ClearValue(TextBlock.FontSizeProperty);
                }
            }
        }

        private static readonly DependencyProperty IsHandlerRegisteredProperty =
            DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveFontSize),
                new PropertyMetadata(false));

        private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
        private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
    }
}