using System.Windows;
using System.Windows.Controls;

namespace Jamesnet.Platform.Wpf.Responsive
{
    public static class ResponsiveFontWeight
    {
        public static readonly DependencyProperty DesktopFontWeightProperty =
            DependencyProperty.RegisterAttached("DesktopFontWeight", typeof(FontWeight?), typeof(ResponsiveFontWeight),
                new PropertyMetadata(null, OnFontWeightChanged));
        public static FontWeight? GetDesktopFontWeight(DependencyObject obj) => (FontWeight?)obj.GetValue(DesktopFontWeightProperty);
        public static void SetDesktopFontWeight(DependencyObject obj, FontWeight? value) => obj.SetValue(DesktopFontWeightProperty, value);

        public static readonly DependencyProperty MobileFontWeightProperty =
            DependencyProperty.RegisterAttached("MobileFontWeight", typeof(FontWeight?), typeof(ResponsiveFontWeight),
                new PropertyMetadata(null, OnFontWeightChanged));
        public static FontWeight? GetMobileFontWeight(DependencyObject obj) => (FontWeight?)obj.GetValue(MobileFontWeightProperty);
        public static void SetMobileFontWeight(DependencyObject obj, FontWeight? value) => obj.SetValue(MobileFontWeightProperty, value);

        public static readonly DependencyProperty BreakpointProperty =
            DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveFontWeight),
                new PropertyMetadata(768.0, OnFontWeightChanged));
        public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
        public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

        private static readonly DependencyProperty IsHandlerRegisteredProperty =
            DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveFontWeight),
                new PropertyMetadata(false));
        private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
        private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);

        private static void OnFontWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if (!IsHandlerRegistered(element))
                {
                    var win = Window.GetWindow(element);
                    if (win != null)
                        win.SizeChanged += (s, _) => UpdateFontWeight(element);

                    element.Loaded += (s, _) =>
                    {
                        var w = Window.GetWindow(element);
                        if (w != null)
                        {
                            w.SizeChanged += (ss, __) => UpdateFontWeight(element);
                            UpdateFontWeight(element);
                        }
                    };

                    RegisterHandler(element);
                }
                UpdateFontWeight(element);
            }
        }

        private static void UpdateFontWeight(FrameworkElement element)
        {
            var win = Window.GetWindow(element);
            if (win == null) return;

            double width = win.ActualWidth;
            double bp = GetBreakpoint(element);
            var desktop = GetDesktopFontWeight(element);
            var mobile = GetMobileFontWeight(element);
            var target = width <= bp ? mobile : desktop;

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
}
