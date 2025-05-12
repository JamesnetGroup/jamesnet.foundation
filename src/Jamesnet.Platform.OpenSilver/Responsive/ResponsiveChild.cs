using System.Windows;
using System.Windows.Controls;

namespace Jamesnet.Platform.OpenSilver.Responsive
{
    public static class ResponsiveChild
    {
        public static readonly DependencyProperty DesktopProperty =
            DependencyProperty.RegisterAttached("Desktop", typeof(UIElement), typeof(ResponsiveChild),
                new PropertyMetadata(null, OnChildChanged));

        public static UIElement GetDesktop(DependencyObject obj) => (UIElement)obj.GetValue(DesktopProperty);
        public static void SetDesktop(DependencyObject obj, UIElement value) => obj.SetValue(DesktopProperty, value);

        public static readonly DependencyProperty MobileProperty =
            DependencyProperty.RegisterAttached("Mobile", typeof(UIElement), typeof(ResponsiveChild),
                new PropertyMetadata(null, OnChildChanged));

        public static UIElement GetMobile(DependencyObject obj) => (UIElement)obj.GetValue(MobileProperty);
        public static void SetMobile(DependencyObject obj, UIElement value) => obj.SetValue(MobileProperty, value);

        public static readonly DependencyProperty BreakpointProperty =
            DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveChild),
                new PropertyMetadata(768.0, OnChildChanged));

        public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
        public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

        private static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border)
            {
                if (!IsHandlerRegistered(border))
                {
                    Window.Current.SizeChanged += (s, args) => UpdateChild(border);
                    border.Loaded += (s, args) => UpdateChild(border);
                    RegisterHandler(border);
                }
                UpdateChild(border);
            }
        }

        private static void UpdateChild(Border border)
        {
            double windowWidth = Window.Current.Bounds.Width;
            double breakpoint = GetBreakpoint(border);
            UIElement defaultChild = GetDesktop(border);
            UIElement mobileChild = GetMobile(border);

            if (windowWidth <= breakpoint)
            {
                if (mobileChild != null)
                    border.Child = mobileChild;
                else
                    border.ClearValue(Border.ChildProperty);
            }
            else
            {
                if (defaultChild != null)
                    border.Child = defaultChild;
                else
                    border.ClearValue(Border.ChildProperty);
            }
        }

        private static readonly DependencyProperty IsHandlerRegisteredProperty =
            DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveChild),
                new PropertyMetadata(false));

        private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
        private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
    }
}