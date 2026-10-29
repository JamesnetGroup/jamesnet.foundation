namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop 값을 BorderThickness에 적용한다. Control과 Border에 붙인다.
/// </summary>
public static class ResponsiveBorderThickness
{
    public static readonly DependencyProperty DesktopProperty =
        ResponsiveHelper.Attach("Desktop", typeof(Thickness), typeof(ResponsiveBorderThickness), new Thickness(0), Apply);

    public static readonly DependencyProperty MobileProperty =
        ResponsiveHelper.Attach("Mobile", typeof(Thickness), typeof(ResponsiveBorderThickness), new Thickness(0), Apply);

    public static readonly DependencyProperty BreakpointProperty =
        ResponsiveHelper.Attach("Breakpoint", typeof(double), typeof(ResponsiveBorderThickness), ResponsiveHelper.DefaultBreakpoint, Apply);

    public static Thickness GetDesktop(DependencyObject obj) => (Thickness)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, Thickness value) => obj.SetValue(DesktopProperty, value);

    public static Thickness GetMobile(DependencyObject obj) => (Thickness)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, Thickness value) => obj.SetValue(MobileProperty, value);

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static Thickness Pick(FrameworkElement element, double width) =>
        ResponsiveHelper.IsMobile(width, GetBreakpoint(element)) ? GetMobile(element) : GetDesktop(element);

    private static void Apply(FrameworkElement element, double width)
    {
        if (element is Control || element is Border)
        {
            element.SetValue(element is Border ? Border.BorderThicknessProperty : Control.BorderThicknessProperty, Pick(element, width));
        }
    }
}
