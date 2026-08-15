#if WINUI3 || UNO
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else // WPF, OPENSILVER
using System.Windows;
using System.Windows.Controls;
#endif
#if WPF
using Jamesnet.Platform.Wpf.Responsive;
#endif

namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop FontSize를 적용하는
/// 플랫폼 중립 attached property. Control과 TextBlock에 붙일 수 있다.
/// 소스는 이 파일 한 벌이고 각 flavor가 컴파일한다.
/// </summary>
public static class ResponsiveFontSize
{
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(double), typeof(ResponsiveFontSize),
            new PropertyMetadata(double.NaN, OnFontSizeChanged));

    public static double GetDesktop(DependencyObject obj) => (double)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, double value) => obj.SetValue(DesktopProperty, value);

    public static readonly DependencyProperty MobileProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(double), typeof(ResponsiveFontSize),
            new PropertyMetadata(double.NaN, OnFontSizeChanged));

    public static double GetMobile(DependencyObject obj) => (double)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, double value) => obj.SetValue(MobileProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveFontSize),
            new PropertyMetadata(768.0, OnFontSizeChanged));

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element || (element is not Control && element is not TextBlock))
        {
            return;
        }

#if WPF
        // WPF는 Window.GetWindow 기반의 ResponsiveHelper가 구독/해제를 관리한다.
        ResponsiveHelper.Register(element, UpdateFontSize);
#else
        if (!IsHandlerRegistered(element))
        {
            Window.Current.SizeChanged += (s, args) => UpdateFontSize(element);
            element.Loaded += (s, args) => UpdateFontSize(element);
            RegisterHandler(element);
        }
        UpdateFontSize(element);
#endif
    }

#if WPF
    private static void UpdateFontSize(FrameworkElement element, double windowWidth)
    {
        ApplyFontSize(element, windowWidth);
    }
#else
    private static void UpdateFontSize(FrameworkElement element)
    {
        ApplyFontSize(element, Window.Current.Bounds.Width);
    }

    // 이벤트 중복 등록 방지
    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveFontSize),
            new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
#endif

    private static void ApplyFontSize(DependencyObject element, double windowWidth)
    {
        double breakpoint = GetBreakpoint(element);
        double defaultFontSize = GetDesktop(element);
        double mobileFontSize = GetMobile(element);

        if (element is Control control)
        {
            if (windowWidth <= breakpoint)
            {
                if (!double.IsNaN(mobileFontSize))
                    control.FontSize = mobileFontSize;
                else
                    control.ClearValue(Control.FontSizeProperty);
            }
            else
            {
                if (!double.IsNaN(defaultFontSize))
                    control.FontSize = defaultFontSize;
                else
                    control.ClearValue(Control.FontSizeProperty);
            }
        }
        else if (element is TextBlock textBlock)
        {
            if (windowWidth <= breakpoint)
            {
                if (!double.IsNaN(mobileFontSize))
                    textBlock.FontSize = mobileFontSize;
                else
                    textBlock.ClearValue(TextBlock.FontSizeProperty);
            }
            else
            {
                if (!double.IsNaN(defaultFontSize))
                    textBlock.FontSize = defaultFontSize;
                else
                    textBlock.ClearValue(TextBlock.FontSizeProperty);
            }
        }
    }
}
