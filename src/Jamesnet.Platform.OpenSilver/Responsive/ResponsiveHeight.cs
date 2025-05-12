using System.Windows;

namespace Jamesnet.Platform.OpenSilver.Responsive
{
    public static class ResponsiveHeight
    {
        public static readonly DependencyProperty DesktopProperty =
            DependencyProperty.RegisterAttached("Desktop", typeof(double), typeof(ResponsiveHeight),
                new PropertyMetadata(double.NaN, OnHeightChanged));

        public static double GetDesktop(DependencyObject obj) => (double)obj.GetValue(DesktopProperty);
        public static void SetDesktop(DependencyObject obj, double value) => obj.SetValue(DesktopProperty, value);

        public static readonly DependencyProperty MobileProperty =
            DependencyProperty.RegisterAttached("Mobile", typeof(double), typeof(ResponsiveHeight),
                new PropertyMetadata(double.NaN, OnHeightChanged));

        public static double GetMobile(DependencyObject obj) => (double)obj.GetValue(MobileProperty);
        public static void SetMobile(DependencyObject obj, double value) => obj.SetValue(MobileProperty, value);

        public static readonly DependencyProperty BreakpointProperty =
            DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveHeight),
                new PropertyMetadata(768.0, OnHeightChanged));

        public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
        public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

        private static void OnHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if (!IsHandlerRegistered(element))
                {
                    Window.Current.SizeChanged += (s, args) => UpdateHeight(element);
                    element.Loaded += (s, args) => UpdateHeight(element);
                    RegisterHandler(element);
                }
                UpdateHeight(element);
            }
        }

        private static void UpdateHeight(FrameworkElement element)
        {
            double windowWidth = Window.Current.Bounds.Width; // Breakpoint 비교를 위해 너비 사용
            double breakpoint = GetBreakpoint(element);
            double defaultHeight = GetDesktop(element);
            double mobileHeight = GetMobile(element);

            if (windowWidth <= breakpoint)
            {
                if (!double.IsNaN(mobileHeight))
                    element.Height = mobileHeight;
                else
                    element.ClearValue(FrameworkElement.HeightProperty);
            }
            else
            {
                if (!double.IsNaN(defaultHeight))
                    element.Height = defaultHeight;
                else
                    element.ClearValue(FrameworkElement.HeightProperty);
            }
        }

        private static readonly DependencyProperty IsHandlerRegisteredProperty =
            DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveHeight),
                new PropertyMetadata(false));

        private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
        private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
    }
}