// UniformGrid는 WPF와 OpenSilver에만 있다. WinUI3/Uno/UWP에는 대응 패널이 없어 이 속성도 없다.
#if WPF || OPENSILVER
using System;

namespace Jamesnet.Foundation.Responsive;

/// <summary>
/// UniformGrid의 실제 너비(ActualWidth)를 BaseWidth로 나눠 Columns를 자동 계산한다.
/// 창 너비가 아니라 그리드 자신의 너비를 기준으로 한다. (v10에서 OpenSilver판 기준으로 통일)
/// </summary>
public static class ResponsiveColumns
{
    public static readonly DependencyProperty BaseWidthProperty =
        DependencyProperty.RegisterAttached("BaseWidth", typeof(double), typeof(ResponsiveColumns), new PropertyMetadata(300.0, OnChanged));

    public static readonly DependencyProperty MinColumnsProperty =
        DependencyProperty.RegisterAttached("MinColumns", typeof(int), typeof(ResponsiveColumns), new PropertyMetadata(1, OnChanged));

    public static readonly DependencyProperty MaxColumnsProperty =
        DependencyProperty.RegisterAttached("MaxColumns", typeof(int), typeof(ResponsiveColumns), new PropertyMetadata(0, OnChanged));

    private static readonly DependencyProperty IsHookedProperty =
        DependencyProperty.RegisterAttached("IsHooked", typeof(bool), typeof(ResponsiveColumns), new PropertyMetadata(false));

    public static double GetBaseWidth(DependencyObject obj) => (double)obj.GetValue(BaseWidthProperty);
    public static void SetBaseWidth(DependencyObject obj, double value) => obj.SetValue(BaseWidthProperty, value);

    public static int GetMinColumns(DependencyObject obj) => (int)obj.GetValue(MinColumnsProperty);
    public static void SetMinColumns(DependencyObject obj, int value) => obj.SetValue(MinColumnsProperty, value);

    public static int GetMaxColumns(DependencyObject obj) => (int)obj.GetValue(MaxColumnsProperty);
    public static void SetMaxColumns(DependencyObject obj, int value) => obj.SetValue(MaxColumnsProperty, value);

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UniformGrid grid)
        {
            return;
        }

        // 그리드 자신의 SizeChanged만 구독하므로 그리드와 함께 수거된다.
        if (!(bool)grid.GetValue(IsHookedProperty))
        {
            grid.SetValue(IsHookedProperty, true);
            grid.SizeChanged += (_, _) => Update(grid);
        }

        Update(grid);
    }

    private static void Update(UniformGrid grid)
    {
        double baseWidth = GetBaseWidth(grid);
        int columns = Math.Max(Math.Max(1, GetMinColumns(grid)), baseWidth > 0 ? (int)(grid.ActualWidth / baseWidth) : 1);
        int max = GetMaxColumns(grid);

        grid.Columns = max > 0 ? Math.Min(columns, max) : columns;
    }
}
#endif
