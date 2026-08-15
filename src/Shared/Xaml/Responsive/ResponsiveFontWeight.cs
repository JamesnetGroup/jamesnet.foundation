#if WINUI3 || UNO
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Text;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Text;
#else // WPF, OPENSILVER
using System.Windows;
using System.Windows.Controls;
#endif
#if WPF
using Jamesnet.Platform.Wpf.Responsive;
#endif

namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop FontWeight를 적용하는
/// 플랫폼 중립 attached property. Control과 TextBlock에 붙일 수 있다.
/// 소스는 이 파일 한 벌이고 각 flavor가 컴파일한다.
/// </summary>
public static class ResponsiveFontWeight
{
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(FontWeight?), typeof(ResponsiveFontWeight),
            new PropertyMetadata(null, OnFontWeightChanged));

    public static FontWeight? GetDesktop(DependencyObject obj) => (FontWeight?)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, FontWeight? value) => obj.SetValue(DesktopProperty, value);

    public static readonly DependencyProperty MobileProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(FontWeight?), typeof(ResponsiveFontWeight),
            new PropertyMetadata(null, OnFontWeightChanged));

    public static FontWeight? GetMobile(DependencyObject obj) => (FontWeight?)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, FontWeight? value) => obj.SetValue(MobileProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveFontWeight),
            new PropertyMetadata(768.0, OnFontWeightChanged));

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnFontWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element || (element is not Control && element is not TextBlock))
        {
            return;
        }

#if WPF
        // WPF는 Window.GetWindow 기반의 ResponsiveHelper가 구독/해제를 관리한다.
        ResponsiveHelper.Register(element, UpdateFontWeight);
#else
        if (!IsHandlerRegistered(element))
        {
            Window.Current.SizeChanged += (s, args) => UpdateFontWeight(element);
            element.Loaded += (s, args) => UpdateFontWeight(element);
            RegisterHandler(element);
        }
        UpdateFontWeight(element);
#endif
    }

#if WPF
    private static void UpdateFontWeight(FrameworkElement element, double windowWidth)
    {
        ApplyFontWeight(element, windowWidth);
    }
#else
    private static void UpdateFontWeight(FrameworkElement element)
    {
        ApplyFontWeight(element, Window.Current.Bounds.Width);
    }

    // 이벤트 중복 등록 방지
    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveFontWeight),
            new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
#endif

    private static void ApplyFontWeight(DependencyObject element, double windowWidth)
    {
        double breakpoint = GetBreakpoint(element);
        var target = windowWidth <= breakpoint ? GetMobile(element) : GetDesktop(element);

        if (element is Control control)
        {
            if (target.HasValue)
                control.FontWeight = target.Value;
            else
                control.ClearValue(Control.FontWeightProperty);
        }
        else if (element is TextBlock textBlock)
        {
            if (target.HasValue)
                textBlock.FontWeight = target.Value;
            else
                textBlock.ClearValue(TextBlock.FontWeightProperty);
        }
    }
}
