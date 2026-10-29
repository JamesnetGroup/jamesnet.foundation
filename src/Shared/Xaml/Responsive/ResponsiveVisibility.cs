namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop 값을 Visibility에 적용한다.
/// </summary>
public static class ResponsiveVisibility
{
    public static readonly DependencyProperty DesktopProperty =
        ResponsiveHelper.Attach("Desktop", typeof(Visibility), typeof(ResponsiveVisibility), Visibility.Visible, Apply);

    public static readonly DependencyProperty MobileProperty =
        ResponsiveHelper.Attach("Mobile", typeof(Visibility), typeof(ResponsiveVisibility), Visibility.Collapsed, Apply);

    public static readonly DependencyProperty BreakpointProperty =
        ResponsiveHelper.Attach("Breakpoint", typeof(double), typeof(ResponsiveVisibility), ResponsiveHelper.DefaultBreakpoint, Apply);

    public static Visibility GetDesktop(DependencyObject obj) => (Visibility)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, Visibility value) => obj.SetValue(DesktopProperty, value);

    public static Visibility GetMobile(DependencyObject obj) => (Visibility)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, Visibility value) => obj.SetValue(MobileProperty, value);

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static Visibility Pick(FrameworkElement element, double width) =>
        ResponsiveHelper.IsMobile(width, GetBreakpoint(element)) ? GetMobile(element) : GetDesktop(element);

    private static void Apply(FrameworkElement element, double width)
    {
        element.Visibility = Pick(element, width);
    }
}
