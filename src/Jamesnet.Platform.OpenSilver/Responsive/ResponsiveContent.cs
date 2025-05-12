using System.Windows;
using System.Windows.Controls;

namespace Jamesnet.Platform.OpenSilver.Responsive
{
    public static class ResponsiveContent
    {
        public static readonly DependencyProperty DesktopProperty =
            DependencyProperty.RegisterAttached("Desktop", typeof(object), typeof(ResponsiveContent),
                new PropertyMetadata(null, OnContentChanged));

        public static object GetDesktop(DependencyObject obj) => obj.GetValue(DesktopProperty);
        public static void SetDesktop(DependencyObject obj, object value) => obj.SetValue(DesktopProperty, value);

        public static readonly DependencyProperty MobileProperty =
            DependencyProperty.RegisterAttached("Mobile", typeof(object), typeof(ResponsiveContent),
                new PropertyMetadata(null, OnContentChanged));

        public static object GetMobile(DependencyObject obj) => obj.GetValue(MobileProperty);
        public static void SetMobile(DependencyObject obj, object value) => obj.SetValue(MobileProperty, value);

        public static readonly DependencyProperty BreakpointProperty =
            DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveContent),
                new PropertyMetadata(768.0, OnContentChanged));

        public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
        public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ContentControl)
            {
                if (!IsHandlerRegistered(d))
                {
                    Window.Current.SizeChanged += (s, args) => UpdateContent(d);
                    if (d is FrameworkElement element)
                    {
                        element.Loaded += (s, args) => UpdateContent(d);
                    }
                    RegisterHandler(d);
                }
                UpdateContent(d);
            }
        }

        private static void UpdateContent(DependencyObject element)
        {
            double windowWidth = Window.Current.Bounds.Width; // Breakpoint 비교를 위해 너비 사용
            double breakpoint = GetBreakpoint(element);
            object defaultContent = GetDesktop(element);
            object mobileContent = GetMobile(element);

            if (element is ContentControl contentControl)
            {
                if (windowWidth <= breakpoint)
                {
                    if (mobileContent != null)
                        contentControl.Content = mobileContent;
                    else
                        contentControl.ClearValue(ContentControl.ContentProperty);
                }
                else
                {
                    if (defaultContent != null)
                        contentControl.Content = defaultContent;
                    else
                        contentControl.ClearValue(ContentControl.ContentProperty);
                }
            }
        }

        private static readonly DependencyProperty IsHandlerRegisteredProperty =
            DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveContent),
                new PropertyMetadata(false));

        private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
        private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
    }
}