using System;
#if WINUI3 || UNO
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
#else // WPF, OPENSILVER
using System.Windows;
using System.Windows.Controls.Primitives;
#endif
#if WPF
using Jamesnet.Platform.Wpf.Responsive;
#endif

namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// UniformGrid의 가용 너비(ActualWidth)를 BaseWidth로 나눠 Columns를 자동 계산하는
/// 플랫폼 중립 attached property. 소스는 이 파일 한 벌이고 각 flavor가 컴파일한다.
/// 기존 WPF판은 창 너비 기준이었으나, OpenSilver판 기준(grid.ActualWidth)으로 통일했다. (v10 breaking change)
/// </summary>
public static class ResponsiveColumns
{
    public static readonly DependencyProperty BaseWidthProperty =
        DependencyProperty.RegisterAttached("BaseWidth", typeof(double), typeof(ResponsiveColumns),
            new PropertyMetadata(300.0, OnColumnsChanged));

    public static double GetBaseWidth(DependencyObject obj) => (double)obj.GetValue(BaseWidthProperty);
    public static void SetBaseWidth(DependencyObject obj, double value) => obj.SetValue(BaseWidthProperty, value);

    public static readonly DependencyProperty MinColumnsProperty =
        DependencyProperty.RegisterAttached("MinColumns", typeof(int), typeof(ResponsiveColumns),
            new PropertyMetadata(1, OnColumnsChanged));

    public static int GetMinColumns(DependencyObject obj) => (int)obj.GetValue(MinColumnsProperty);
    public static void SetMinColumns(DependencyObject obj, int value) => obj.SetValue(MinColumnsProperty, value);

    public static readonly DependencyProperty MaxColumnsProperty =
        DependencyProperty.RegisterAttached("MaxColumns", typeof(int), typeof(ResponsiveColumns),
            new PropertyMetadata(0, OnColumnsChanged));

    public static int GetMaxColumns(DependencyObject obj) => (int)obj.GetValue(MaxColumnsProperty);
    public static void SetMaxColumns(DependencyObject obj, int value) => obj.SetValue(MaxColumnsProperty, value);

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UniformGrid grid)
        {
            return;
        }

#if WPF
        // WPF는 Window.GetWindow 기반의 ResponsiveHelper가 구독/해제를 관리한다.
        ResponsiveHelper.Register(grid, UpdateColumns);
#else
        if (!IsHandlerRegistered(grid))
        {
            grid.SizeChanged += (s, args) => UpdateColumns(grid);
            grid.Loaded += (s, args) => UpdateColumns(grid);
            RegisterHandler(grid);
        }
        UpdateColumns(grid);
#endif
    }

#if WPF
    // helper가 넘겨주는 창 너비는 쓰지 않고 grid.ActualWidth를 쓴다 (OpenSilver판 기준으로 통일).
    private static void UpdateColumns(FrameworkElement element, double windowWidth)
    {
        UpdateColumns((UniformGrid)element);
    }
#endif

    private static void UpdateColumns(UniformGrid grid)
    {
        double width = grid.ActualWidth;
        double baseWidth = GetBaseWidth(grid);
        int minColumns = GetMinColumns(grid);
        int maxColumns = GetMaxColumns(grid);

        int calculatedColumns = (int)Math.Floor(width / baseWidth);

        int columns = Math.Max(minColumns, calculatedColumns);
        if (maxColumns > 0)
            columns = Math.Min(columns, maxColumns);

        grid.Columns = columns;
    }

#if !WPF
    // 이벤트 중복 등록 방지
    private static readonly DependencyProperty IsHandlerRegisteredProperty =
        DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveColumns),
            new PropertyMetadata(false));

    private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
    private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
#endif
}
