#if WPF || OPENSILVER
using Weight = System.Nullable<System.Windows.FontWeight>;
#else
// WinUI 계열의 FontWeight는 Windows.UI.Text의 구조체다. Weight 0(기본값)을 "지정 안 함"으로 쓴다.
using Weight = Windows.UI.Text.FontWeight;
#endif

namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop FontWeight를 적용한다.
/// Control과 TextBlock에 붙인다. 값을 지정하지 않은 쪽은 스타일·상속 FontWeight로 되돌린다.
/// </summary>
public static class ResponsiveFontWeight
{
    public static readonly DependencyProperty DesktopProperty =
        ResponsiveHelper.Attach("Desktop", typeof(Weight), typeof(ResponsiveFontWeight), default(Weight), Apply);

    public static readonly DependencyProperty MobileProperty =
        ResponsiveHelper.Attach("Mobile", typeof(Weight), typeof(ResponsiveFontWeight), default(Weight), Apply);

    public static readonly DependencyProperty BreakpointProperty =
        ResponsiveHelper.Attach("Breakpoint", typeof(double), typeof(ResponsiveFontWeight), ResponsiveHelper.DefaultBreakpoint, Apply);

    public static Weight GetDesktop(DependencyObject obj) => (Weight)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, Weight value) => obj.SetValue(DesktopProperty, value);

    public static Weight GetMobile(DependencyObject obj) => (Weight)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, Weight value) => obj.SetValue(MobileProperty, value);

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void Apply(FrameworkElement element, double width)
    {
        if (element is not Control && element is not TextBlock)
        {
            return;
        }

        Weight value = ResponsiveHelper.IsMobile(width, GetBreakpoint(element)) ? GetMobile(element) : GetDesktop(element);
#if WPF || OPENSILVER
        object boxed = value;
#else
        object boxed = value.Weight == 0 ? null : value;
#endif
        ResponsiveHelper.SetOrClear(element, element is TextBlock ? TextBlock.FontWeightProperty : Control.FontWeightProperty, boxed);
    }
}
