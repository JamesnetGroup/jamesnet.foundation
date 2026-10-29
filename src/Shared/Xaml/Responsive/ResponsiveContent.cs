namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop 값을 ContentControl.Content에 적용한다. ContentControl에 붙인다. 값이 null이면 직접 지정한 Content를 지우고 스타일·상속 값으로 되돌린다.
/// </summary>
public static class ResponsiveContent
{
    public static readonly DependencyProperty DesktopProperty =
        ResponsiveHelper.Attach("Desktop", typeof(object), typeof(ResponsiveContent), null, Apply);

    public static readonly DependencyProperty MobileProperty =
        ResponsiveHelper.Attach("Mobile", typeof(object), typeof(ResponsiveContent), null, Apply);

    public static readonly DependencyProperty BreakpointProperty =
        ResponsiveHelper.Attach("Breakpoint", typeof(double), typeof(ResponsiveContent), ResponsiveHelper.DefaultBreakpoint, Apply);

    public static object GetDesktop(DependencyObject obj) => (object)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, object value) => obj.SetValue(DesktopProperty, value);

    public static object GetMobile(DependencyObject obj) => (object)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, object value) => obj.SetValue(MobileProperty, value);

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static object Pick(FrameworkElement element, double width) =>
        ResponsiveHelper.IsMobile(width, GetBreakpoint(element)) ? GetMobile(element) : GetDesktop(element);

    private static void Apply(FrameworkElement element, double width)
    {
        if (element is ContentControl)
        {
            ResponsiveHelper.SetOrClear(element, ContentControl.ContentProperty, Pick(element, width));
        }
    }
}
