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
/// 창 너비가 Breakpoint 이하이면 Mobile, 아니면 Desktop Content를 적용하는
/// 플랫폼 중립 attached property. ContentControl에 붙일 수 있다.
/// 소스는 이 파일 한 벌이고 각 flavor가 컴파일한다.
/// </summary>
public static class ResponsiveContent
{
    public static readonly DependencyProperty DesktopProperty =
        DependencyProperty.RegisterAttached("Desktop", typeof(object), typeof(ResponsiveContent),
            new PropertyMetadata(null, OnContentChanged));

    public static object GetDesktop(DependencyObject obj) => obj.GetValue(DesktopProperty);
    public static void SetDesktop(DependencyObject obj, object value) => obj.SetValue(DesktopProperty, value);

    public static readonly DependencyProperty MobileProperty =
        DependencyProperty.RegisterAttached("Mobile", typeof(object), typeof(ResponsiveContent),
            new PropertyMetadata(null, OnContentChanged));

    public static object GetMobile(DependencyObject obj) => obj.GetValue(MobileProperty);
    public static void SetMobile(DependencyObject obj, object value) => obj.SetValue(MobileProperty, value);

    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached("Breakpoint", typeof(double), typeof(ResponsiveContent),
            new PropertyMetadata(768.0, OnContentChanged));

    public static double GetBreakpoint(DependencyObject obj) => (double)obj.GetValue(BreakpointProperty);
    public static void SetBreakpoint(DependencyObject obj, double value) => obj.SetValue(BreakpointProperty, value);

    private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ContentControl element)
        {
            return;
        }

#if WPF
        // WPF는 Window.GetWindow 기반의 ResponsiveHelper가 구독/해제를 관리한다.
        ResponsiveHelper.Register(element, UpdateContent);
#else
        if (!IsHandlerRegistered(element))
        {
            Window.Current.SizeChanged += (s, args) => UpdateContent(element);
            element.Loaded += (s, args) => UpdateContent(element);
            RegisterHandler(element);
        }
        UpdateContent(element);
#endif
    }

#if WPF
    private static void UpdateContent(FrameworkElement element, double windowWidth)
    {
        ApplyContent(element, windowWidth);
    }
#else
    private static void UpdateContent(FrameworkElement element)
    {
        ApplyContent(element, Window.Current.Bounds.Width);
    }

    // 이벤트 중복 등록 방지
    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveContent),
            new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
#endif

    private static void ApplyContent(DependencyObject element, double windowWidth)
    {
        double breakpoint = GetBreakpoint(element);
        object defaultContent = GetDesktop(element);
        object mobileContent = GetMobile(element);

        if (element is ContentControl contentControl)
        {
            if (windowWidth <= breakpoint)
            {
                if (mobileContent != null)
                    contentControl.Content = mobileContent;
                else
                    contentControl.ClearValue(ContentControl.ContentProperty);
            }
            else
            {
                if (defaultContent != null)
                    contentControl.Content = defaultContent;
                else
                    contentControl.ClearValue(ContentControl.ContentProperty);
            }
        }
    }
}
