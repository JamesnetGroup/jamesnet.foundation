using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Jamesnet.Platform.Wpf.Responsive
{
    public static class ResponsiveAnimationTo
    {
        public static readonly DependencyProperty DesktopToProperty =
            DependencyProperty.RegisterAttached("DesktopTo", typeof(double), typeof(ResponsiveAnimationTo),
                new PropertyMetadata(double.NaN, OnToValueChanged));

        public static double GetDesktopTo(DependencyObject obj) => (double)obj.GetValue(DesktopToProperty);

        public static void SetDesktopTo(DependencyObject obj, double value) =>
            obj.SetValue(DesktopToProperty, value);

        public static readonly DependencyProperty MobileToProperty =
            DependencyProperty.RegisterAttached("MobileTo", typeof(double), typeof(ResponsiveAnimationTo),
                new PropertyMetadata(double.NaN, OnToValueChanged));

        public static double GetMobileTo(DependencyObject obj) => (double)obj.GetValue(MobileToProperty);

        public static void SetMobileTo(DependencyObject obj, double value) =>
            obj.SetValue(MobileToProperty, value);

        public static readonly DependencyProperty BreakpointProperty =
            DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveAnimationTo),
                new PropertyMetadata(768.0, OnToValueChanged));

        public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);

        public static void SetBreakpoint(DependencyObject obj, double value) =>
            obj.SetValue(BreakpointProperty, value);

        private static readonly DependencyProperty TargetWindowProperty =
            DependencyProperty.RegisterAttached("TargetWindow", typeof(Window), typeof(ResponsiveAnimationTo),
                new PropertyMetadata(null));

        private static void SetTargetWindow(DependencyObject obj, Window value) =>
            obj.SetValue(TargetWindowProperty, value);

        private static Window GetTargetWindow(DependencyObject obj) =>
            (Window)obj.GetValue(TargetWindowProperty);

        private static void OnToValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DoubleAnimation animation)
            {
                if (!IsHandlerRegistered(animation))
                {
                    Window window = FindWindow(animation);
                    if (window != null)
                    {
                        SetTargetWindow(animation, window);
                        window.SizeChanged += (s, args) => UpdateToValue(animation);
                        RegisterHandler(animation);
                    }
                }

                UpdateToValue(animation);
            }
        }

        private static Window FindWindow(DependencyObject animation)
        {
            return Application.Current?.MainWindow;
        }

        private static void UpdateToValue(DoubleAnimation animation)
        {
            Window window = GetTargetWindow(animation);
            if (window == null) return;

            double windowWidth = window.ActualWidth;
            double breakpoint = GetBreakpoint(animation);
            double desktopToValue = GetDesktopTo(animation);
            double mobileToValue = GetMobileTo(animation);

            if (windowWidth <= breakpoint)
            {
                if (!double.IsNaN(mobileToValue))
                    animation.To = mobileToValue;
                else if (!double.IsNaN(desktopToValue))
                    animation.To = desktopToValue;
            }
            else
            {
                if (!double.IsNaN(desktopToValue))
                    animation.To = desktopToValue;
                else if (!double.IsNaN(mobileToValue))
                    animation.To = mobileToValue;
            }
        }

        private static readonly DependencyProperty IsHandlerRegisteredProperty =
            DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveAnimationTo),
                new PropertyMetadata(false));

        private static bool IsHandlerRegistered(DependencyObject obj) =>
            (bool)obj.GetValue(IsHandlerRegisteredProperty);

        private static void RegisterHandler(DependencyObject obj) =>
            obj.SetValue(IsHandlerRegisteredProperty, true);
    }
}