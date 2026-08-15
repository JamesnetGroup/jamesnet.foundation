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
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop Child를 적용하는
/// 플랫폼 중립 attached property. Border에 붙일 수 있다.
/// 소스는 이 파일 한 벌이고 각 flavor가 컴파일한다.
/// </summary>
public static class ResponsiveChild
{
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(UIElement), typeof(ResponsiveChild),
            new PropertyMetadata(null, OnChildChanged));

    public static UIElement GetDesktop(DependencyObject obj) => (UIElement)obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, UIElement value) => obj.SetValue(DesktopProperty, value);

    public static readonly DependencyProperty MobileProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(UIElement), typeof(ResponsiveChild),
            new PropertyMetadata(null, OnChildChanged));

    public static UIElement GetMobile(DependencyObject obj) => (UIElement)obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, UIElement value) => obj.SetValue(MobileProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveChild),
            new PropertyMetadata(768.0, OnChildChanged));

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Border element)
        {
            return;
        }

#if WPF
        // WPF는 Window.GetWindow 기반의 ResponsiveHelper가 구독/해제를 관리한다.
        ResponsiveHelper.Register(element, UpdateChild);
#else
        if (!IsHandlerRegistered(element))
        {
            Window.Current.SizeChanged += (s, args) => UpdateChild(element);
            element.Loaded += (s, args) => UpdateChild(element);
            RegisterHandler(element);
        }
        UpdateChild(element);
#endif
    }

#if WPF
    private static void UpdateChild(FrameworkElement element, double windowWidth)
    {
        ApplyChild(element, windowWidth);
    }
#else
    private static void UpdateChild(FrameworkElement element)
    {
        ApplyChild(element, Window.Current.Bounds.Width);
    }

    // 이벤트 중복 등록 방지
    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveChild),
            new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
#endif

    private static void ApplyChild(DependencyObject element, double windowWidth)
    {
        double breakpoint = GetBreakpoint(element);
        UIElement defaultChild = GetDesktop(element);
        UIElement mobileChild = GetMobile(element);

        if (element is Border border)
        {
            if (windowWidth <= breakpoint)
            {
                if (mobileChild != null)
                    border.Child = mobileChild;
                else
                    border.ClearValue(Border.ChildProperty);
            }
            else
            {
                if (defaultChild != null)
                    border.Child = defaultChild;
                else
                    border.ClearValue(Border.ChildProperty);
            }
        }
    }
}
