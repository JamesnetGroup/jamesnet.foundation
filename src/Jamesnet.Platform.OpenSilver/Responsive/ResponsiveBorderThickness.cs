using System.Windows;
using System.Windows.Controls;

namespace Jamesnet.Platform.OpenSilver.Responsive
{
    public static class ResponsiveBorderThickness
    {
        public static readonly DependencyProperty DesktopProperty =
            DependencyProperty.RegisterAttached("Desktop", typeof(Thickness), typeof(ResponsiveBorderThickness),
                new PropertyMetadata(new Thickness(0), OnBorderThicknessChanged));

        public static Thickness GetDesktop(DependencyObject obj) => (Thickness)obj.GetValue(DesktopProperty);
        public static void SetDesktop(DependencyObject obj, Thickness value) => obj.SetValue(DesktopProperty, value);

        public static readonly DependencyProperty MobileProperty =
            DependencyProperty.RegisterAttached("Mobile", typeof(Thickness), typeof(ResponsiveBorderThickness),
                new PropertyMetadata(new Thickness(0), OnBorderThicknessChanged));

        public static Thickness GetMobile(DependencyObject obj) => (Thickness)obj.GetValue(MobileProperty);
        public static void SetMobile(DependencyObject obj, Thickness value) => obj.SetValue(MobileProperty, value);

        public static readonly DependencyProperty BreakpointProperty =
            DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveBorderThickness),
                new PropertyMetadata(768.0, OnBorderThicknessChanged));

        public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
        public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

        private static void OnBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control || d is Border)
            {
                if (!IsHandlerRegistered(d))
                {
                    Window.Current.SizeChanged += (s, args) => UpdateBorderThickness(d);
                    if (d is FrameworkElement element)
                    {
                        element.Loaded += (s, args) => UpdateBorderThickness(d);
                    }
                    RegisterHandler(d);
                }
                UpdateBorderThickness(d);
            }
        }

        private static void UpdateBorderThickness(DependencyObject element)
        {
            double windowWidth = Window.Current.Bounds.Width; // Breakpoint 비교를 위해 너비 사용
            double breakpoint = GetBreakpoint(element);
            Thickness defaultBorderThickness = GetDesktop(element);
            Thickness mobileBorderThickness = GetMobile(element);

            if (element is Control control)
            {
                if (windowWidth <= breakpoint)
                {
                    control.BorderThickness = mobileBorderThickness;
                }
                else
                {
                    control.BorderThickness = defaultBorderThickness;
                }
            }
            else if (element is Border border)
            {
                if (windowWidth <= breakpoint)
                {
                    border.BorderThickness = mobileBorderThickness;
                }
                else
                {
                    border.BorderThickness = defaultBorderThickness;
                }
            }
        }

        private static readonly DependencyProperty IsHandlerRegisteredProperty =
            DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveBorderThickness),
                new PropertyMetadata(false));

        private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
        private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
    }
}