namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop 값을 FontSize에 적용한다. Control과 TextBlock에 붙인다. 값이 NaN이면 직접 지정한 FontSize를 지우고 스타일·상속 값으로 되돌린다.
/// </summary>
public static class ResponsiveFontSize
{
    public static readonly DependencyProperty DesktopProperty =
        ResponsiveHelper.Attach("Desktop", typeof(double), typeof(ResponsiveFontSize), double.NaN, Apply);

    public static readonly DependencyProperty MobileProperty =
        ResponsiveHelper.Attach("Mobile", typeof(double), typeof(ResponsiveFontSize), double.NaN, Apply);

    public static readonly DependencyProperty BreakpointProperty =
        ResponsiveHelper.Attach("Breakpoint", typeof(double), typeof(ResponsiveFontSize), ResponsiveHelper.DefaultBreakpoint, Apply);

    public static double GetDesktop(DependencyObject obj) => (double)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, double value) => obj.SetValue(DesktopProperty, value);

    public static double GetMobile(DependencyObject obj) => (double)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, double value) => obj.SetValue(MobileProperty, value);

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static double Pick(FrameworkElement element, double width) =>
        ResponsiveHelper.IsMobile(width, GetBreakpoint(element)) ? GetMobile(element) : GetDesktop(element);

    private static void Apply(FrameworkElement element, double width)
    {
        if (element is Control || element is TextBlock)
        {
            ResponsiveHelper.SetOrClear(element, element is TextBlock ? TextBlock.FontSizeProperty : Control.FontSizeProperty, Pick(element, width));
        }
    }
}
