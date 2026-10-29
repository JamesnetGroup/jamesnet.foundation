namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop 값을 Border.Child에 적용한다. Border에 붙인다.
/// </summary>
public static class ResponsiveChild
{
    public static readonly DependencyProperty DesktopProperty =
        ResponsiveHelper.Attach("Desktop", typeof(UIElement), typeof(ResponsiveChild), null, Apply);

    public static readonly DependencyProperty MobileProperty =
        ResponsiveHelper.Attach("Mobile", typeof(UIElement), typeof(ResponsiveChild), null, Apply);

    public static readonly DependencyProperty BreakpointProperty =
        ResponsiveHelper.Attach("Breakpoint", typeof(double), typeof(ResponsiveChild), ResponsiveHelper.DefaultBreakpoint, Apply);

    public static UIElement GetDesktop(DependencyObject obj) => (UIElement)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, UIElement value) => obj.SetValue(DesktopProperty, value);

    public static UIElement GetMobile(DependencyObject obj) => (UIElement)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, UIElement value) => obj.SetValue(MobileProperty, value);

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static UIElement Pick(FrameworkElement element, double width) =>
        ResponsiveHelper.IsMobile(width, GetBreakpoint(element)) ? GetMobile(element) : GetDesktop(element);

    private static void Apply(FrameworkElement element, double width)
    {
        if (element is Border border)
        {
            border.Child = Pick(element, width);
        }
    }
}
