using System.Windows;

namespace Jamesnet.Platform.OpenSilver.Responsive
{
    public static class ResponsiveWidth
    {
        public static readonly DependencyProperty DesktopProperty =
            DependencyProperty.RegisterAttached("Desktop", typeof(double), typeof(ResponsiveWidth),
                new PropertyMetadata(double.NaN, OnWidthChanged));

        public static double GetDesktop(DependencyObject obj) => (double)obj.GetValue(DesktopProperty);
        public static void SetDesktop(DependencyObject obj, double value) => obj.SetValue(DesktopProperty, value);

        public static readonly DependencyProperty MobileWidthProperty =
            DependencyProperty.RegisterAttached("Mobile", typeof(double), typeof(ResponsiveWidth),
                new PropertyMetadata(double.NaN, OnWidthChanged));

        public static double GetMobile(DependencyObject obj) => (double)obj.GetValue(MobileWidthProperty);
        public static void SetMobile(DependencyObject obj, double value) => obj.SetValue(MobileWidthProperty, value);

        public static readonly DependencyProperty BreakpointProperty =
            DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveWidth),
                new PropertyMetadata(768.0, OnWidthChanged));

        public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
        public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

        private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if (!IsHandlerRegistered(element))
                {
                    Window.Current.SizeChanged += (s, args) => UpdateWidth(element);
                    element.Loaded += (s, args) => UpdateWidth(element);
                    RegisterHandler(element);
                }
                UpdateWidth(element);
            }
        }

        private static void UpdateWidth(FrameworkElement element)
        {
            double windowWidth = Window.Current.Bounds.Width;
            double breakpoint = GetBreakpoint(element);
            double defaultWidth = GetDesktop(element);
            double mobileWidth = GetMobile(element);

            if (windowWidth <= breakpoint)
            {
                if (!double.IsNaN(mobileWidth))
                    element.Width = mobileWidth; 
                else
                    element.ClearValue(FrameworkElement.WidthProperty);
            }
            else
            {
                if (!double.IsNaN(defaultWidth))
                    element.Width = defaultWidth;
                else
                    element.ClearValue(FrameworkElement.WidthProperty);
            }
        }

        private static readonly DependencyProperty IsHandlerRegisteredProperty =
            DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveWidth),
                new PropertyMetadata(false));

        private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
        private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
    }
}



