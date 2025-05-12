using System.Windows;

namespace Jamesnet.Platform.OpenSilver.Responsive
{
    public static class ResponsiveMinHeight
    {
        public static readonly DependencyProperty DesktopProperty =
            DependencyProperty.RegisterAttached("Desktop", typeof(double), typeof(ResponsiveMinHeight),
                new PropertyMetadata(double.NaN, OnMinHeightChanged));

        public static double GetDesktop(DependencyObject obj) => (double)obj.GetValue(DesktopProperty);
        public static void SetDesktop(DependencyObject obj, double value) => obj.SetValue(DesktopProperty, value);

        public static readonly DependencyProperty MobileProperty =
            DependencyProperty.RegisterAttached("Mobile", typeof(double), typeof(ResponsiveMinHeight),
                new PropertyMetadata(double.NaN, OnMinHeightChanged));

        public static double GetMobile(DependencyObject obj) => (double)obj.GetValue(MobileProperty);
        public static void SetMobile(DependencyObject obj, double value) => obj.SetValue(MobileProperty, value);

        public static readonly DependencyProperty BreakpointProperty =
            DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveMinHeight),
                new PropertyMetadata(768.0, OnMinHeightChanged));

        public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
        public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

        private static void OnMinHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if (!IsHandlerRegistered(element))
                {
                    Window.Current.SizeChanged += (s, args) => UpdateMinHeight(element);
                    element.Loaded += (s, args) => UpdateMinHeight(element);
                    RegisterHandler(element);
                }
                UpdateMinHeight(element);
            }
        }

        private static void UpdateMinHeight(FrameworkElement element)
        {
            double windowWidth = Window.Current.Bounds.Width;
            double breakpoint = GetBreakpoint(element);
            double defaultMinHeight = GetDesktop(element);
            double mobileMinHeight = GetMobile(element);

            if (windowWidth <= breakpoint)
            {
                if (!double.IsNaN(mobileMinHeight))
                    element.MinHeight = mobileMinHeight;
                else
                    element.ClearValue(FrameworkElement.MinHeightProperty);
            }
            else
            {
                if (!double.IsNaN(defaultMinHeight))
                    element.MinHeight = defaultMinHeight;
                else
                    element.ClearValue(FrameworkElement.MinHeightProperty);
            }
        }

        private static readonly DependencyProperty IsHandlerRegisteredProperty =
            DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveMinHeight),
                new PropertyMetadata(false));

        private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
        private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
    }
}