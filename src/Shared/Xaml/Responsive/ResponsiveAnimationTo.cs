namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// 창 너비가 Breakpoint 이하이면 MobileTo, 아니면 DesktopTo를 DoubleAnimation.To에 적용한다.
/// 한쪽만 지정하면 양쪽 모두 그 값을 쓴다.
///
/// DoubleAnimation은 화면 요소가 아니어서 자기 창을 알 수 없으므로 앱 단위 너비를 쓴다.
/// (WPF는 MainWindow, OpenSilver/UWP는 Window.Current, WinUI3/Uno는 화면의 Responsive* 요소가 감지한 너비)
/// </summary>
public static class ResponsiveAnimationTo
{
    public static readonly DependencyProperty DesktopToProperty =
        DependencyProperty.RegisterAttached("DesktopTo", typeof(double), typeof(ResponsiveAnimationTo), new PropertyMetadata(double.NaN, OnChanged));

    public static readonly DependencyProperty MobileToProperty =
        DependencyProperty.RegisterAttached("MobileTo", typeof(double), typeof(ResponsiveAnimationTo), new PropertyMetadata(double.NaN, OnChanged));

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveAnimationTo), new PropertyMetadata(ResponsiveHelper.DefaultBreakpoint, OnChanged));

    public static double GetDesktopTo(DependencyObject obj) => (double)obj.GetValue(DesktopToProperty);
    public static void SetDesktopTo(DependencyObject obj, double value) => obj.SetValue(DesktopToProperty, value);

    public static double GetMobileTo(DependencyObject obj) => (double)obj.GetValue(MobileToProperty);
    public static void SetMobileTo(DependencyObject obj, double value) => obj.SetValue(MobileToProperty, value);

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DoubleAnimation)
        {
            ResponsiveHelper.AppWidth.Track(d, Apply);
        }
    }

    private static void Apply(DependencyObject d, double width)
    {
        double desktop = GetDesktopTo(d);
        double mobile = GetMobileTo(d);
        double to = ResponsiveHelper.IsMobile(width, GetBreakpoint(d))
            ? (double.IsNaN(mobile) ? desktop : mobile)
            : (double.IsNaN(desktop) ? mobile : desktop);

#if WPF
        if (((Freezable)d).IsFrozen)
        {
            return; // 이미 시작된 Storyboard 안의 애니메이션은 고정(frozen)되어 바꿀 수 없다.
        }
#endif
        if (!double.IsNaN(to))
        {
            ((DoubleAnimation)d).To = to;
        }
    }
}
